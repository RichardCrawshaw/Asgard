using System;
using Asgard.Data.Interfaces;

namespace Asgard.Communications
{
    public delegate void CbusEventCallback(ICbusAccessoryEvent cbusAccessoryEvent);

    public interface ICbusEventManager
    {
        bool? GetState(ushort nodeNumber, ushort eventNumber);

        bool? GetState(ushort eventNumber);

        void RegisterCbusEvent<T>(ushort nodeNumber, ushort eventNumber, CbusEventCallback callback)
            where T : class, ICbusAccessoryEvent;

        bool TryGetState(ushort nodeNumber, ushort eventNumber, out bool state);

        bool TryGetState(ushort eventNumber, out bool state);
    }
}