﻿using Asgard.Data.Interfaces;

namespace Asgard.Data
{
    public partial class AccessoryShortOn3 :
    ICbusAccessoryShortEvent,
    ICbusAccessoryOnEvent
    {
        public bool IsLongEvent => false;
        public bool IsShortEvent => true;
        public bool IsOnEvent => true;
        public bool IsOffEvent => false;
    }
}
