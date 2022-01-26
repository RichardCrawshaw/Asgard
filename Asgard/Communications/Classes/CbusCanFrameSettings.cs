using System;
using Asgard.Extensions;

namespace Asgard.Communications
{
    public class CbusCanFrameSettings
    {
        public byte? CanId { get; set; }

        public string? MajorPriority { get; set; }
        public string? MinorPriority { get; set; }

        public MajorPriority? GetMajorPriority() => this.MajorPriority?.Get<MajorPriority>();

        public MinorPriority? GetMinorPriority() => this.MinorPriority?.Get<MinorPriority>();

        public void SetMajorPriority(MajorPriority value) => this.MajorPriority = Enum.GetName(value);

        public void SetMinorPriority(MinorPriority value) => this.MinorPriority = Enum.GetName(value);
    }
}
