using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public abstract class StreamTransport : 
        ITransport
    {
        private readonly ILogger<StreamTransport> logger;

        protected Stream TransportStream { get; set; }

        public StreamTransport(ILogger<StreamTransport> logger = null)
        {
            this.logger = logger;
        }

        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            this.logger?.LogTrace("Reading from stream");
            try
            {
                return await this.TransportStream.ReadAsync(buffer, cancellationToken);
            }
            catch (IOException ex)
            {
                this.logger?.LogError("IO read error: {0}", ex.Message);
                return 0;
            }
            catch (OperationCanceledException ex)
            {
                this.logger?.LogError("Cancellation read : {0}", ex.Message);
                if (cancellationToken.IsCancellationRequested)
                    // Let the handling of receiving nothing automatically deal with cancellation.
                    return 0;

                // An external cancellation hasn't been requested; so it looks like the port has 
                // been disconnected; attempt to reconnect.
                var reconnected = Reconnect(cancellationToken);
                if (!reconnected)
                    // Reconnection has failed, so force the read handling to drop out.
                    return -1;

                return 0;
            }
        }

        public async Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            this.logger?.LogTrace("Writing to stream");
            this.logger?.LogDebug("Writing {0} bytes", buffer.Length);
            await this.TransportStream.WriteAsync(buffer, cancellationToken);
            await this.TransportStream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Helper routine to manage the reconnection to the underlying transport.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow managed cancellation to occur.</param>
        /// <returns>True if reconnection was successful; False otherwise.</returns>
        /// <remarks>
        /// If reconnection was not successful the calling method should tidy up and quit.
        /// </remarks>
        protected bool Reconnect(CancellationToken cancellationToken)
        {
            //if (this.TransportStream == null) return false;

            this.logger?.LogInformation("Waiting for reconnection...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var state = Reopen();
                    if (state)
                    {
                        this.logger?.LogInformation("Reconnection successful.");
                        return true;
                    }
                }
                catch (IOException)
                {
                    // This exception will happen all the time the Transport is trying to
                    // reconnect.
                }
                catch (OperationCanceledException)
                {
                    // Things are being cancelled, so abandon all attempts to reconnect.
                    return false;
                }

                // Sleep for a second so we don't overwhelm things.
                Thread.Sleep(1000);
                // TODO: handle this using the cancellation token.
            }

            return false;
        }

        public abstract void Open(CancellationToken cancellationToken);

        protected abstract bool Reopen();
    }
}
