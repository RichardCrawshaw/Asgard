using System;
using System.Net;

namespace Asgard.Comms
{
    public interface ISocketAdaptor :
        IDisposable
    {
        #region Properties

        string Address { get; set; }

        long? Handle { get; }

        IPAddress IPAddress { get; set; }

        bool IsConnected { get; }

        bool IsDisposed { get; }

        int Port { get; set; }

        #endregion

        #region Methods

        bool Send(string text);

        bool Send(byte[] data);

        #endregion

        #region Events

        event EventHandler<DataReceivedEventArgs> DataReceived;

        #endregion
    }
}
