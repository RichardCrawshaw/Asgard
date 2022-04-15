namespace Asgard.Data
{

    /// <summary>
    /// Interface to describe a CBUS message object.
    /// </summary>
    public interface ICbusMessage
    {
        bool IsExtended { get; }

        int Length { get; }

        byte this[int index] { get; set; }
    }

    public interface ICbusStandardMessage :
        ICbusMessage
    {
        bool TryGetOpCode(out ICbusOpCode opCode);
    }

    public interface ICbusExtendedMessage :
        ICbusMessage
    {

    }
}
