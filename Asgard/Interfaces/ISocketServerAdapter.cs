using System.Net.Sockets;

namespace Asgard
{
    public interface ISocketServerAdapter :
        ISocketAdaptor
    {
        #region Methods

        void Disconnect(Socket socket);

        void Start();

        void Stop();

        #endregion
    }
}
