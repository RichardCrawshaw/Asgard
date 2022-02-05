namespace Asgard.Data.Interfaces
{
    public interface ICbusAccessoryEvent
    {
        bool IsLongEvent { get; }
        bool IsShortEvent { get; }
        bool IsOnEvent { get; }
        bool IsOffEvent { get; }
    }

    public interface ICbusAccessoryLongEvent :
        ICbusAccessoryEvent
    { }

    public interface ICbusAccessoryShortEvent :
        ICbusAccessoryEvent
    { }

    public interface ICbusAccessoryOnEvent :
        ICbusAccessoryEvent
    { }

    public interface ICbusAccessoryOffEvent :
        ICbusAccessoryEvent
    { }
}
