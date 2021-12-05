using System;

namespace Asgard.Comms
{
    public interface ICommsAdapter :
        IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets whether the current instance is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets whether the current instance has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets the unified name of the current instance.
        /// </summary>
        string Name { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Connect the current instance to the underlying communication channel.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect the current instance from the currently connected underlying communication channel.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/> containing the data to send.</param>
        void Send(byte[] data);

        /// <summary>
        /// Sends the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to send.</param>
        void Send(string text);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when data has been received.
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;

        #endregion
    }
}
