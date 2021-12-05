using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Data;

namespace Asgard.Communications
{
    public class CbusCanFrame:ICbusCanFrame
    {
        public byte SidH { get; set; }
        public byte SidL { get; set; }
        public FrameTypes FrameType { get; set; }
        public MajorPriority MajorPriority
        {
            get => (MajorPriority)(SidH >> 6);
            set => SidH = (byte)(((byte)value << 6) + (SidH & 0x3f));
        }

        public MinorPriority MinorPriority
        {
            get => (MinorPriority)(SidH >> 4 & 0x3);
            set => SidH = (byte)(((byte)value << 4) + (SidH & 0xcf));
        }

        public byte CanId
        {
            get => (byte)((SidH << 8) + SidL >> 5 & 0x7f);
            set
            {
                SidH = (byte)((value >> 3) + (SidH & 0xF0));
                SidL = (byte)(value << 5 & 0xFF);
            }
        }
        public ICbusMessage Message { get; set; }

        public override string ToString()
        {
            //TODO: construct a reasonable looking frame representation for logging purposes
            return base.ToString();
        }
    }
}
