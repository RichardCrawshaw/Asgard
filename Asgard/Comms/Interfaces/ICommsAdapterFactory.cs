namespace Asgard.Comms
{
    internal interface ICommsAdapterFactory
    {
        ICommsAdapter CreateSerialPortAdapter();
        ICommsAdapter CreateSocketClientAdapter();
    }
}
