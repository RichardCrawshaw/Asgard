using System;
using System.IO;
using System.IO.Ports;

namespace Asgard
{
    internal interface ISerialPortAdapter :
        IDisposable
    {
        #region Properties

        bool IsDisposed { get; }

        int BaudRate { get; }

        uint BufferSize { get; set; }

        int DataBits { get; }

        bool IsOpen { get; }

        Parity Parity { get; }

        string PortName { get; }

        StopBits StopBits { get; }

        Stream Stream { get; }

        #endregion

        #region Methods

        void Close();

        void Open(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity);

        void Send(byte[] buffer);

        void Write(string text);

        #endregion

        #region Events

        event EventHandler<ReceivedSerialDataEventArgs> ReceivedSerialData;

        event EventHandler<SerialPortErrorEventArgs> SerialPortError;

        #endregion
    }
}
