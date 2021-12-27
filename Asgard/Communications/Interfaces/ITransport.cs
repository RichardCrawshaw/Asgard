using System;
using System.Threading;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public interface ITransport
    {
        ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken);
        Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

        void Open(CancellationToken cancellationToken);

        // TODO: Events for (dis)connection?
    }
}
