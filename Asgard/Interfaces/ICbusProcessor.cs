using System;

namespace Asgard
{
    public interface ICbusProcessor :
        IDisposable
    {
        #region Properties

        bool IsConnected { get; }

        bool IsDisposed { get; }

        #endregion

        #region Methods

        void Connect();

        void Disconnect();

        #endregion
    }
}
