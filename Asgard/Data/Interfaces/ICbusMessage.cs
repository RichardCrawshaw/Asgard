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

        bool TryGetOpCode(out ICbusOpCode opCode);
    }
}
