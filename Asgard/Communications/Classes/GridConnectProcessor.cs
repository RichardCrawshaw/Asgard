﻿using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class GridConnectProcessor : 
        IGridConnectProcessor, 
        IDisposable
    {
        private readonly ILogger<GridConnectProcessor>? logger;

        public ITransport? Transport { get; }

        public event EventHandler<TransportErrorEventArgs>? TransportError;
        public event EventHandler<MessageReceivedEventArgs>? GridConnectMessage;

        private CancellationTokenSource? cts;
        private bool disposedValue;

        public GridConnectProcessor(ITransport transport, ILogger<GridConnectProcessor>? logger = null)
        {
            this.Transport = transport;
            this.logger = logger;
        }

        public async Task OpenAsync()
        {
            if (this.Transport is null) return;

            this.logger?.LogTrace(nameof(OpenAsync));
            this.cts = new CancellationTokenSource();

            await this.Transport.OpenAsync(this.cts.Token);
            if (this.cts.Token.IsCancellationRequested)
                return;

            var pipe = new Pipe();
            //TODO: would this be better as distinct threads rather than using up threads from the threadpool?
            ReadPipe(pipe.Reader).AsgardFireAndForget();
            Listen(pipe.Writer).AsgardFireAndForget();
        }

        public bool Close()
        {
            this.logger?.LogTrace(nameof(Close));

            // Force all the various async processes to stop, tidy up and quit.
            this.cts?.Cancel();

            return true;
        }

        private Task Listen(PipeWriter writer) => Task.Run(() => ListenAsync(writer));

        private async void ListenAsync(PipeWriter writer)
        {
            const int minBufferSize = 64;

            if (this.Transport is null) return;
            if (this.cts is null) return;

            while (!this.cts.Token.IsCancellationRequested)
            {
                var memory = writer.GetMemory(minBufferSize);
                try
                {
                    var read = await this.Transport.ReadAsync(memory, this.cts.Token);

                    // Check for cancellation.
                    if (this.cts.Token.IsCancellationRequested) break;

                    // Check for whether we should continue reading or not.
                    if (read == -1) break;

                    // Nothing has been received so just try again.
                    if (read == 0) continue; ;

                    this.logger?.LogTrace("Read {count} bytes", read);

                    writer.Advance(read);
                    await writer.FlushAsync(this.cts.Token);
                }
                catch (IOException ex)
                {
                    this.logger?.LogError(ex, "IO error reading from transport.");
                }
                catch (TaskCanceledException)
                {
                    // Catch the cancelled exception and just let the loop terminate normally.
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error reading from transport");
                    TransportError?.Invoke(this,
                        new TransportErrorEventArgs(
                            new TransportException("Error receiving bytes", ex)));
                }
            }
        }

        private Task ReadPipe(PipeReader reader) => Task.Run(() => ReadAsync(reader));

        private async void ReadAsync(PipeReader reader)
        {
            if (this.cts is null) return;

            while (!this.cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await reader.ReadAsync(this.cts.Token);
                    var buffer = result.Buffer;
                    SequencePosition? endPosition;
                    do
                    {
                        endPosition = buffer.PositionOf((byte)';');
                        if (endPosition != null)
                        {
                            endPosition = buffer.GetPosition(1, endPosition.Value);
                            ProcessMessage(buffer.Slice(0, endPosition.Value));
                            buffer = buffer.Slice(endPosition.Value);
                        }

                    } while (endPosition != null);

                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
                catch (OperationCanceledException)
                {
                    // Catch the cancelled exception and just let the loop terminate normally.
                }
            }
        }

        private void ProcessMessage(ReadOnlySequence<byte> readOnlySequence)
        {
            var startPosition = readOnlySequence.PositionOf((byte)':');
            if (startPosition != null)
            {
                var msg = GetMessageString(readOnlySequence.Slice(startPosition.Value, readOnlySequence.End));
                this.logger?.LogTrace("Message received {message}", msg);
                this.GridConnectMessage?.Invoke(this, new MessageReceivedEventArgs(msg));
            }
            else if (this.logger is not null || this.TransportError is not null)
            {
                var message = GetMessageString(readOnlySequence);
                this.logger?.LogWarning("{message}", message);
                this.TransportError?.Invoke(this,
                    new TransportErrorEventArgs(
                        new TransportException(message)));
            }
        }

        private static string GetMessageString(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return Encoding.ASCII.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.ASCII.GetChars(segment.Span, span);
                    span = span[segment.Length..];
                }
            });
        }

        public async Task SendMessage(string gridConnectMessage)
        {
            if (this.Transport is null) return;
            if (this.cts is null) return;

            try
            {
                this.logger?.LogTrace("Sending message: {message}", gridConnectMessage);
                await this.Transport.SendAsync(Encoding.ASCII.GetBytes(gridConnectMessage), this.cts.Token);

            }
            catch (TaskCanceledException)
            {
                //ok
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Error sending message");
                TransportError?.Invoke(this,
                    new TransportErrorEventArgs(
                        new TransportException("Error sending message", ex)));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.logger?.LogTrace("Disposing: {flag}", disposing);
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.cts?.Dispose();
                    this.cts = null;
                }
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
