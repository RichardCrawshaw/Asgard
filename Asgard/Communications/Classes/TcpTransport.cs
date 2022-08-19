using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class TcpTransport : StreamTransport, IDisposable
    {
        private readonly TcpTransportSettings settings;
        private readonly ILogger<TcpTransport>? logger;

        private TcpClient? tcpClient;
        private bool disposedValue;

        public TcpTransport(TcpTransportSettings settings, ILogger<TcpTransport>? logger) : base(logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(settings.Host, settings.Port);
            this.TransportStream = tcpClient.GetStream();
        }
        protected override bool Reopen() {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
            }
            tcpClient = new TcpClient();
            tcpClient.Connect(settings.Host, settings.Port);
            this.TransportStream = tcpClient.GetStream();
            return true;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    tcpClient?.Dispose();
                    tcpClient = null;
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
