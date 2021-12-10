using System;
using System.IO.Ports;

namespace Asgard.Communications
{
    public class SerialPortTransport:StreamTransport, IDisposable
    {
        private readonly SerialPortTransportSettings settings;
        private SerialPort port;
        private bool disposedValue;

        public SerialPortTransport(SerialPortTransportSettings settings)
        {
            this.settings = settings;
        }

        public override void Open()
        {
            port = new SerialPort(settings.PortName);
            port.Open();
            TransportStream = port.BaseStream;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    port.Dispose();
                    port = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
