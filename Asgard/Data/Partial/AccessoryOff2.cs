using Asgard.Data.Interfaces;

namespace Asgard.Data
{
    public partial class AccessoryOff2 :
    ICbusAccessoryLongEvent,
    ICbusAccessoryOffEvent
    {
        public bool IsLongEvent => true;
        public bool IsShortEvent => false;
        public bool IsOnEvent => false;
        public bool IsOffEvent => true;
    }
}
