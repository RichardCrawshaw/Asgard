namespace Asgard.Communications
{
    public interface ICbusConnectionFactory
    {
        IGridConnectProcessor GetConnection();
        IGridConnectProcessor GetConnection(ConnectionOptions connectionOptions);
        string[] GetAvailableConnections();
    }
}
