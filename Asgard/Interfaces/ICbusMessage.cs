namespace Asgard
{
    /// <summary>
    /// Interface to describe a CBUS message object.
    /// </summary>
    public interface ICbusMessage
    {
        byte[] Data { get; }

        int Length { get; }

        byte this[int index] { get; set; }

        ICbusOpCode GetOpCode();
    }
}
