using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Ports;

namespace Asgard.Communications
{
    public class SerialPortTransport:StreamTransport, IDisposable
    {
        private readonly SerialPortTransportSettings settings;
        private readonly ILogger<SerialPortTransport> logger;
        private SerialPort port;
        private bool disposedValue;

        public SerialPortTransport(SerialPortTransportSettings settings, ILogger<SerialPortTransport> logger = null) : base(logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public override void Open()
        {
            logger?.LogInformation("Opening serial port: {0}", settings.PortName);
            port = new SerialPort(settings.PortName);
            try
            {
                port.Open();
            }
            catch(FileNotFoundException e)
            {
                logger?.LogError(@"Unable to open serial port - ""{0}"" not found", e.FileName);
                throw new TransportException($"The selected SerialPort could not be found: {e.FileName}", e);
            }
            TransportStream = port.BaseStream;
        }
        protected virtual void Dispose(bool disposing)
        {
            logger?.LogTrace("Disposing: {0}", disposing);
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
