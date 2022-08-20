namespace Asgard.Communications
{
    public class ConnectionOptions
    {
        public enum ConnectionTypes
        {
            SerialPort,
            Tcp
        }

        public ConnectionTypes ConnectionType { get; set; }

        public SerialPortTransportSettings? SerialPort { get; set; }

        public TcpTransportSettings? Tcp { get; set; }
    }
}