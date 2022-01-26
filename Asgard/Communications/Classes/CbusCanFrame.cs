using Asgard.Data;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class CbusCanFrame : 
        ICbusCanFrame
    {
        private readonly CbusCanFrameSettings? settings;
        private readonly ILogger<CbusCanFrame>? logger;

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

        public ICbusMessage? Message { get; set; }

        public CbusCanFrame(CbusCanFrameSettings? settings, ILogger<CbusCanFrame>? logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public void Instantiate(ICbusOpCode cbusOpCode)
        {
            // TODO: Extract the major and minor priority from the op-code meta-data.

            this.CanId = this.settings?.CanId ?? 125;
            this.MajorPriority = this.settings?.GetMajorPriority() ?? MajorPriority.Low;
            this.MinorPriority = this.settings?.GetMinorPriority() ?? MinorPriority.Normal;

            this.logger?.LogInformation($"Created CAN frame for {cbusOpCode?.Code}");
        }

        public override string ToString() => 
            $"0x{this.SidL:X2} 0x{this.SidH:X2} (0x{this.CanId:X2}) {this.FrameType} {this.Message}";
    }
}
