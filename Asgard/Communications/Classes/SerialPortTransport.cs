using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Ports;

namespace Asgard.Communications
{
    public class SerialPortTransport : StreamTransport, 
        IDisposable
    {
        private readonly SerialPortTransportSettings settings;
        private readonly ILogger<SerialPortTransport> logger;

        private SerialPort port;
        private bool disposedValue;

        public SerialPortTransport(SerialPortTransportSettings settings, ILogger<SerialPortTransport> logger = null) 
            : base(logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public override void Open()
        {
            this.logger?.LogInformation("Opening serial port: {0}", this.settings.PortName);
            this.port = new SerialPort(this.settings.PortName);

            try
            {
                this.port.Open();
            }
            catch(FileNotFoundException e)
            {
                this.logger?.LogError(@"Unable to open serial port - ""{0}"" not found", e.FileName);
                throw new TransportException($"The selected SerialPort could not be found: {e.FileName}", e);
            }
            this.TransportStream = this.port.BaseStream;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.logger?.LogTrace("Disposing: {0}", disposing);
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.port.Dispose();
                    this.port = null;
                }
                this.disposedValue = true;
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
