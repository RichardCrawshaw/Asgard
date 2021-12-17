namespace Asgard.Communications
{
    public interface ICbusCanFrameProcessor
    {
        CbusCanFrame ParseFrame(string transportString);
        string ConstructTransportString(CbusCanFrame frame);
    }
}
