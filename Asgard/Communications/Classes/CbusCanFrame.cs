using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public static CbusCanFrame FromTransportString(string transportString)
        {

            var p = 1;
            if (transportString[p] != 'S')
            {
                //non-standard, don't support yet
                throw new NotImplementedException();
            }

            p++;
            var sidh = Convert.ToByte(transportString.Substring(p, 2), 16);
            p += 2;
            var sidl = Convert.ToByte(transportString.Substring(p, 2), 16);
            p += 2;
            var frametype = transportString[p] == 'N' ? FrameTypes.Normal : FrameTypes.Rtr;
            p++;

            var dataBytes = new byte[(transportString.Length - p - 1) / 2];
            for (var x = 0; p < transportString.Length - 2; p += 2, x++)
            {
                dataBytes[x] = Convert.ToByte(transportString.Substring(p, 2), 16);
            }

            var frame = new CbusCanFrame();
            frame.SidH = sidh;
            frame.SidL = sidl;
            frame.FrameType = frametype;

            //TODO: consider making a CbusMessageFactory to decouple this dependency
            frame.Message = CbusMessage.Create(dataBytes);

            return frame;

        }

        public string CreateTransportString()
        {

            var ts = new StringBuilder(8 + (Message.Length*2));
            ts.Append(":S");
            ts.Append(SidH.ToString("X2"));
            ts.Append(SidL.ToString("X2"));
            ts.Append("N");
            for (var x = 0; x < Message.Length; x++)
            {
                ts.Append(Message[x].ToString("X2"));
            }
            ts.Append(";");
            return ts.ToString();

        }
    }
}
