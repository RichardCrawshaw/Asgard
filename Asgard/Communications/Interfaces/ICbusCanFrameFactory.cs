using Asgard.Data;

namespace Asgard.Communications
{
    public interface ICbusCanFrameFactory
    {
        ICbusCanFrame CreateFrame(ICbusMessage message);

        // TODO: pass in settings.
    }
}
