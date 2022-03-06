using Asgard.Data;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class CbusExtendedCanFrame : CbusCanFrame,
        ICbusExtendedCanFrame
    {
        public byte EidH { get; set; }
        public byte EidL { get; set; }

        public CbusExtendedCanFrame(ICbusMessage message,
                                    CbusCanFrameSettings? settings = null,
                                    ILogger<CbusCanFrame>? logger = null)
            : base(message, settings, logger) => this.IsExtended = true;

        public override string ToString() =>
            $"0x{this.SidL:X2} 0x{this.SidH:X2} 0x{this.EidH:X2} 0x{this.EidL:X2} (0x{this.CanId:X2}) {this.FrameType} {this.Message}";
    }
}
