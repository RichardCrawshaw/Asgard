using System;

namespace Asgard
{
    public interface IGridConnectProcessor :
        IDisposable
    {
        #region Properties

        bool IsDisposed { get; }

        bool IsConnected { get; }

        #endregion

        #region Methods

        bool Connect();

        bool Disconnect();

        #endregion

        #region Events

        event EventHandler<GridConnectMessageEventArgs> MessageReceived;

        #endregion
    }
}
