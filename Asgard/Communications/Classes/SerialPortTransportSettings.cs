using System;
using System.IO.Ports;
using Asgard.Extensions;

namespace Asgard.Communications
{
    public class SerialPortTransportSettings
    {
        public string? PortName { get; set; }
        public int? BaudRate { get; set; }
        public int? DataBits { get; set; }
        public string? StopBits { get; set; }
        public string? Parity { get; set; }

        public Parity? GetParity() => this.Parity?.Get<Parity>();

        public StopBits? GetStopBits() => this.StopBits?.Get<StopBits>();

        public void SetParity(Parity parity) => this.Parity = Enum.GetName(parity);

        public void SetStopBits(StopBits stopBits) => this.StopBits = Enum.GetName(stopBits);
    }
}
