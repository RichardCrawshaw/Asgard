using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Microsoft.Extensions.Logging;

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

        /// <summary>
        /// Attempts to open the underlying serial port.
        /// </summary>
        /// <exception cref="TransportException">If the selected serial port could not be found.</exception>
        public override void Open(CancellationToken cancellationToken)
        {
            this.logger?.LogInformation("Opening serial port: {0}", this.settings.PortName);
            this.port = GetSerialPort();

            try
            {
                this.port.Open();
                this.TransportStream = this.port.BaseStream;
            }
            catch (FileNotFoundException e)
            {
                this.logger?.LogError(@"Unable to open serial port - ""{0}"" not found", e.FileName);
                LogAvailablePorts();

                // Wait for the port to become available.
                var reconnected = Reconnect(cancellationToken);
                if (!reconnected)
                    throw new TransportException($"The selected SerialPort could not be found: {e.FileName}", e);
            }
        }

        /// <summary>
        /// Attemp to (re)open the Serial Port but with minimal logging as this routine is 
        /// typically called when the port has disconnected, so it will spend most of its time
        /// failing.
        /// </summary>
        /// <returns>True if the port was re-opened successfully; false otherwise.</returns>
        protected override bool Reopen()
        {
            // Ensure that any previous port has been disposed of prior to creating a new one.
            this.port?.Dispose();

            this.port = GetSerialPort();

            try
            {
                this.port.Open();
                this.TransportStream = this.port.BaseStream;
                return true;
            }
            catch (FileNotFoundException)
            {
                // Don't bother logging anything, as this routine is likely to be called repeatedly
                // and we don't want the logs to fill up with the same message.
                return false;
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, "Attempting to re-open {0}.", this.settings.PortName);
                throw;
            }
        }

        /// <summary>
        /// Creates and returns an instance of a <see cref="SerialPort"/> using the values in 
        /// <seealso cref="settings"/>.
        /// </summary>
        /// <returns>A <see cref="SerialPort"/> object.</returns>
        private SerialPort GetSerialPort()
        {
            var serialPort =
                new SerialPort(this.settings.PortName)
                {
                    BaudRate = this.settings.BaudRate,
                };
            return serialPort;
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

        /// <summary>
        /// Helper routine to log the available serial ports.
        /// </summary>
        private void LogAvailablePorts()
        {
            var portNames = SerialPort.GetPortNames();
            this.logger?.LogInformation("Found {0} COM ports:", portNames.Length);
            foreach (var name in portNames)
                this.logger?.LogInformation(name);
        }
    }
}
