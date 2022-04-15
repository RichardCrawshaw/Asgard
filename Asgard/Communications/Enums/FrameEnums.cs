using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public enum CanFrameTypes
    {
        Undefined = 0,

        Standard,
        Extended,
    }

    public enum FrameTypes
    {
        Normal,
        Rtr
    }

    public enum MajorPriority
    {
        High = 0,       //0x00
        Medium = 1,     //0x01
        Low = 2         //0x10
    }

    public enum MinorPriority
    {
        High = 0,
        AboveNormal = 1,
        Normal = 2,
        Low = 3
    }
}
