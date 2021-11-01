namespace Asgard
{
    internal interface ICommsAdapterFactory
    {
        ICommsAdapter CreateSerialPortAdapter();
        ICommsAdapter CreateSocketClientAdapter();
    }
}
