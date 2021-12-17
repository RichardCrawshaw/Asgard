using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public abstract class StreamTransport : ITransport
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
            return await this.TransportStream.ReadAsync(buffer, cancellationToken);
        }

        public async Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            this.logger?.LogTrace("Writing to stream");
            this.logger?.LogDebug("Writing {0} bytes", buffer.Length);
            await this.TransportStream.WriteAsync(buffer, cancellationToken);
            await this.TransportStream.FlushAsync();
        }

        public abstract void Open();
       
    }
}
