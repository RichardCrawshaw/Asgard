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
            logger?.LogTrace("Reading from stream");
            return await TransportStream.ReadAsync(buffer, cancellationToken);
        }

        public async Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            logger?.LogTrace("Writing to stream");
            logger?.LogDebug("Writing {0} bytes", buffer.Length);
            await TransportStream.WriteAsync(buffer, cancellationToken);
            await TransportStream.FlushAsync();
        }

        public abstract void Open();
       
    }
}
