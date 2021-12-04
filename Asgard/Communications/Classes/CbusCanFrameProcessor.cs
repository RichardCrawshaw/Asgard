using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    internal class CbusCanFrameProcessor : ICbusCanFrameProcessor
    {
        private readonly ILogger<CbusCanFrameProcessor> logger;

        public CbusCanFrameProcessor(ILogger<CbusCanFrameProcessor> logger = null)
        {
            this.logger = logger;
        }
        public string ConstructTransportString(CbusCanFrame frame)
        {
            logger?.LogTrace("Creating transport string for {0}", frame);
            var ts = new StringBuilder(8 + (frame.Message.Length * 2));
            ts.Append(":S");
            ts.Append(frame.SidH.ToString("X2"));
            ts.Append(frame.SidL.ToString("X2"));
            ts.Append("N");
            for (var x = 0; x < frame.Message.Length; x++)
            {
                ts.Append(frame.Message[x].ToString("X2"));
            }
            ts.Append(";");
            return ts.ToString();
        }

        public CbusCanFrame ParseFrame(string transportString)
        {
            logger?.LogTrace("Parsing frame from transport string: {0}", transportString);
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
    }
}
