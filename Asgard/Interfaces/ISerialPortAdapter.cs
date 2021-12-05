using System;
using System.IO;
using System.IO.Ports;

namespace Asgard.Comms
{
    public interface ISerialPortAdapter :
        ICommsAdapter
    {
        #region Properties

        int BaudRate { get; set; }

        uint BufferSize { get; set; }

        int DataBits { get; set; }

        Parity Parity { get; set; }

        string PortName { get; set; }

        StopBits StopBits { get; set; }

        Stream Stream { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when there is a serial port error.
        /// </summary>
        event EventHandler<SerialPortErrorEventArgs> SerialPortError;

        #endregion
    }
}
