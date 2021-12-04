using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public abstract class StreamTransport : ITransport
    {
        protected Stream TransportStream { get; set; }

        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return await TransportStream.ReadAsync(buffer, cancellationToken);
        }

        public async Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            await TransportStream.WriteAsync(buffer, cancellationToken);
            await TransportStream.FlushAsync();
        }

        public abstract void Open();
       
    }
}
