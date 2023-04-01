using Asgard.Data.Interfaces;

namespace Asgard.Data
{
    public partial class AccessoryOn1 :
    ICbusAccessoryLongEvent,
    ICbusAccessoryOnEvent
    {
        public bool IsLongEvent => true;
        public bool IsShortEvent => false;
        public bool IsOnEvent => true;
        public bool IsOffEvent => false;
    }
}
