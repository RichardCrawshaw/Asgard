using System;

namespace Asgard.Comms
{
    /// <summary>
    /// An event args class to carry the exception generated when a serial port reports an error.
    /// </summary>
    public class SerialPortErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The exception that has been generated.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
