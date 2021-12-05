using Asgard.Data;

namespace Asgard.Communications
{
    public interface ICbusCanFrame
    {
        byte CanId { get; set; }
        byte SidH { get; set; }
        byte SidL { get; set; }
        FrameTypes FrameType { get; set; }
        MajorPriority MajorPriority { get; set; }
        MinorPriority MinorPriority { get; set; }
        ICbusMessage Message { get; set; }
    }
}
