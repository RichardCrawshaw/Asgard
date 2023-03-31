using Asgard.Data.Interfaces;

namespace Asgard.Data
{
    public partial class AccessoryShortOff1 :
    ICbusAccessoryShortEvent,
    ICbusAccessoryOffEvent
    {
        public bool IsLongEvent => false;
        public bool IsShortEvent => true;
        public bool IsOnEvent => false;
        public bool IsOffEvent => true;
    }
}
