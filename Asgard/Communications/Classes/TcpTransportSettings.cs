using System.Net;

namespace Asgard.Communications
{
    public class TcpTransportSettings
    {
        public int Port { get; set; }
        public string Host { get; set; } = "localhost";
    }
}