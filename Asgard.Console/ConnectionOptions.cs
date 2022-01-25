using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class ConnectionOptions:Dialog
    {
        public string SelectedPort { get; private set; } = "";
        private string[] _availablePorts = Array.Empty<string>();

        internal void Initialise()
        {
            this.Title = "Select Port";
            this.Width = 40;
            this.Height = 10;
            SelectedPort = "";

            //_availablePorts = new string[] { "COM1", "COM2", "COM3", "COM4" };
            _availablePorts = SerialPort.GetPortNames();

            var lv = new ListView(_availablePorts)
            {
                X = Pos.Center(),
                Y = 1,
                Width = 10,
                Height = 5,
                ColorScheme = Colors.TopLevel
            };

            this.Add(lv);

            var connect = new Button("Connect") {
                IsDefault = true
            };

            var cancel = new Button("Cancel");

            cancel.Clicked += () => {
                Application.RequestStop(this);
            };

            connect.Clicked += () => {
                SelectedPort = _availablePorts[lv.SelectedItem];
                Application.RequestStop(this);
            };

            this.AddButton(connect);
            this.AddButton(cancel);
            
        }
    }
}
