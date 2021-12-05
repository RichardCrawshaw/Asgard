namespace Asgard.Data
{

    /// <summary>
    /// Interface to describe a CBUS message object.
    /// </summary>
    public interface ICbusMessage
    {
        int Length { get; }

        byte this[int index] { get; set; }

        ICbusOpCode GetOpCode();
    }
}
