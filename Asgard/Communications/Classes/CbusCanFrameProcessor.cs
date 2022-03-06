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

        public CbusCanFrame? ParseFrame(string transportString)
        {
            this.logger?.LogTrace("Parsing frame from transport string: {0}", transportString);
            var p = 1;
            var canFrameType = transportString[p] switch
            {
                'S' => CanFrameTypes.Standard,
                'X' => CanFrameTypes.Extended,
                _ => CanFrameTypes.Undefined,
            };

            if (canFrameType == CanFrameTypes.Standard)
            {
                return ParseFrameStandard(p, transportString);
            }

            if (canFrameType == CanFrameTypes.Extended)
            {
                return ParseFrameExtended(p, transportString);
            }

            // Ignore all non-Standard and non-Extended frames.
            return null;
        }

        private static CbusCanFrame? ParseFrameStandard(int p, string transportString)
        {
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

            var message = CbusMessage.Create(dataBytes, isExtended: false);
            if (message is null) return null;
            var frame = new CbusCanFrame(message)
            {
                SidH = sidh,
                SidL = sidl,
                FrameType = frametype,
            };

            return frame;
        }

        private static CbusExtendedCanFrame? ParseFrameExtended(int p, string transportString)
        {
            p++;
            var sidh = Convert.ToByte(transportString.Substring(p, 2), 16);
            p += 2;
            var sidl = Convert.ToByte(transportString.Substring(p, 2), 16);
            p += 2;
            var eidh = Convert.ToByte(transportString.Substring(p, 2), 16);
            p += 2;
            var eidl = Convert.ToByte(transportString.Substring(p, 2), 16);
            p += 2;
            var frametype = transportString[p] == 'N' ? FrameTypes.Normal : FrameTypes.Rtr;
            p++;

            var dataBytes = new byte[8];
            for (var x = 0; p < transportString.Length - 2; p += 2, x++)
                dataBytes[x] = Convert.ToByte(transportString.Substring(p, 2), 16);

            var message = CbusMessage.Create(dataBytes, isExtended: true);
            if (message is null) return null;
            var frame = new CbusExtendedCanFrame(message)
            {
                SidH = sidh,
                SidL = sidl,
                EidH = eidh,
                EidL = eidl,
                FrameType = frametype,
            };

            return frame;
        }
    }
}
