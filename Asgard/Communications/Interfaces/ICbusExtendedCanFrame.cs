namespace Asgard.Communications
{
    internal interface ICbusExtendedCanFrame :
        ICbusCanFrame
    {
        byte EidH { get; set; }
        byte EidL { get; set; }
    }
}
