using System;
using System.Text;
using Asgard.Data;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    internal class CbusCanFrameProcessor :
        ICbusCanFrameProcessor
    {
        private readonly ILogger<CbusCanFrameProcessor>? logger;

        public CbusCanFrameProcessor(ILogger<CbusCanFrameProcessor>? logger = null)
        {
            this.logger = logger;
        }

        public string ConstructTransportString(ICbusCanFrame frame)
        {
            this.logger?.LogTrace("Creating transport string for {0}", frame);

            var message = frame.Message;
            if (message is null) return string.Empty;

            var ts = new StringBuilder(8 + message.Length * 2);
            ts.Append(":S");
            ts.Append(frame.SidH.ToString("X2"));
            ts.Append(frame.SidL.ToString("X2"));
            ts.Append('N');
            for (var x = 0; x < message.Length; x++)
            {
                ts.Append(message[x].ToString("X2"));
            }
            ts.Append(';');
            return ts.ToString();
        }

        public CbusCanFrame ParseFrame(string transportString)
        {
            this.logger?.LogTrace("Parsing frame from transport string: {0}", transportString);
            var p = 1;
            if (transportString[p] != 'S')
            {
                //non-standard, don't support yet
                throw new NotImplementedException("Only standard frames are supported.");
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

            var frame = new CbusCanFrame(null, null)
            {
                SidH = sidh,
                SidL = sidl,
                FrameType = frametype,

                //TODO: consider making a CbusMessageFactory to decouple this dependency
                Message = CbusMessage.Create(dataBytes)
            };

            return frame;
        }
    }
}
