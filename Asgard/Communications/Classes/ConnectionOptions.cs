namespace Asgard.Communications
{
    public class ConnectionOptions
    {
        public enum ConnectionTypes
        {
            SerialPort
        }

        public ConnectionTypes ConnectionType { get; set; }

        public SerialPortTransportSettings SerialPort { get; set; }
    }
}