using System;
using System.IO.Ports;
using System.Runtime.Versioning;
using Terminal.Gui;

namespace Asgard.Console
{
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("windows")]
    internal class ConnectionOptions : Dialog
    {
        public Communications.ConnectionOptions? Connection { get; private set; }
        private string[] _availablePorts = Array.Empty<string>();

        internal void Initialise()
        {
            this.Title = "Select Connection";
            this.Width = 40;
            this.Height = 15;

            var rb = new RadioGroup
            {
                X = Pos.Center(),
                Y = 1,
                RadioLabels = new NStack.ustring[] { "Serial", "TCP" }
            };
            this.Add(rb);
            rb.SelectedItem = 0;


            //_availablePorts = new string[] { "COM1", "COM2", "COM3", "COM4" };
            _availablePorts = SerialPort.GetPortNames();

            var lv = new ListView(_availablePorts)
            {
                X = Pos.Center(),
                Y = 5,
                Width = 10,
                Height = 5,
                ColorScheme = Colors.TopLevel
            };

            this.Add(lv);

            var hostLabel = new Label
            {
                Text = "Host",
                X = 10,
                Y = 4,
                Visible = false
            };
            this.Add(hostLabel);
            var host = new TextField
            {
                X = 15,
                Y = 4,
                Width = 16,
                Text = "localhost",
                Visible = false
            };
            this.Add(host);

            var portLabel = new Label
            {
                Text = "Port",
                X = 10,
                Y = 6,
                Visible = false
            };
            this.Add(portLabel);
            var port = new TextField
            {
                X = 15,
                Y = 6,
                Width = 5,
                Text = "5550",
                Visible = false
            };
            this.Add(port);

            rb.SelectedItemChanged += (s) =>
            {
                switch (rb.SelectedItem)
                {
                    case 0:
                        lv.Visible = true;
                        host.Visible = false;
                        port.Visible = false;
                        hostLabel.Visible = false;
                        portLabel.Visible = false;
                        break;
                    case 1:
                        lv.Visible = false;
                        host.Visible = true;
                        port.Visible = true;
                        hostLabel.Visible = true;
                        portLabel.Visible = true;
                        break;
                }
                this.SetNeedsDisplay();
            };

            var connect = new Button("Connect") {
                IsDefault = true
            };

            var cancel = new Button("Cancel");

            cancel.Clicked += () => {
                Application.RequestStop(this);
            };

            connect.Clicked += () => {
                this.Connection = new Communications.ConnectionOptions();
                switch (rb.SelectedItem)
                {
                    case 0:
                        Connection.ConnectionType = Communications.ConnectionOptions.ConnectionTypes.SerialPort;
                        Connection.SerialPort = new Communications.SerialPortTransportSettings { PortName = _availablePorts[lv.SelectedItem] };
                        break;
                    case 1:
                        var h = host?.Text.ToString() ?? "localhost";
                        if (!short.TryParse(port.Text.ToString(), out var p))
                        {
                            p = 5550;
                        }
                        Connection.ConnectionType = Communications.ConnectionOptions.ConnectionTypes.Tcp;
                        Connection.Tcp = new Communications.TcpTransportSettings { Host = h, Port = p };
                        break;
                }
                
                Application.RequestStop(this);
            };

            this.AddButton(connect);
            this.AddButton(cancel);
            
        }
    }
}
