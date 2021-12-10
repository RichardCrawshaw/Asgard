namespace Asgard.Comms
{
    public interface ISocketClientAdapter :
        ISocketAdaptor
    {
        #region Methods

        void Connect();

        void Disconnect();

        #endregion
    }
}
