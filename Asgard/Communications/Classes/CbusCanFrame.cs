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
            get => (MajorPriority)(this.SidH >> 6);
            set => this.SidH = (byte)(((byte)value << 6) + (this.SidH & 0x3f));
        }

        public MinorPriority MinorPriority
        {
            get => (MinorPriority)(this.SidH >> 4 & 0x3);
            set => this.SidH = (byte)(((byte)value << 4) + (this.SidH & 0xcf));
        }

        public byte CanId
        {
            get => (byte)((this.SidH << 8) + this.SidL >> 5 & 0x7f);
            set
            {
                this.SidH = (byte)((value >> 3) + (this.SidH & 0xF0));
                this.SidL = (byte)(value << 5 & 0xFF);
            }
        }

        public ICbusMessage Message { get; set; }

        public override string ToString() => 
            $"0x{this.SidL:X2} 0x{this.SidH:X2} (0x{this.CanId:X2}) {this.FrameType} {this.Message}";
    }
}
