using System;

namespace Asgard
{
    /// <summary>
    /// An event args class to carry data received by a serial port.
    /// </summary>
    public class ReceivedSerialDataEventArgs : EventArgs
    {
        /// <summary>
        /// The data that has been received.
        /// </summary>
        public byte[] ReceivedSerialData { get; set; }
    }
}
