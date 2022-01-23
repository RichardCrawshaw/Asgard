using Asgard.Communications;
using Asgard.EngineControl;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class DriveLoco : Window
    {
        private ICbusMessenger cbusMessenger;
        private EngineManager engineManager;

        private TextField locoAddress;
        private TextField locoSpeed;
        private TextField locoFunction;
        private CheckBox reverse;
        private EngineSession engineSession;
        private Window locoControl;

        public DriveLoco(ICbusMessenger cbusMessenger)
        {
            this.cbusMessenger = cbusMessenger;
            this.engineManager = new EngineManager(cbusMessenger);


            this.Add(new Label() { Text = "Address: ", X = 0, Y = 0 });

            locoAddress = new TextField()
            {
                Text = "5",
                X = 9,
                Y = 0,
                Width = 5,
            };
            this.Add(locoAddress);

            var share = new CheckBox()
            {
                Text = "Share",
                X = 9,
                Y = 1
            };
            this.Add(share);

            var steal = new CheckBox()
            {
                Text = "Steal",
                X = 9,
                Y = 2
            };
            this.Add(steal);

            var connect = new Button()
            {
                Text = "Get Session",
                X = 0,
                Y = 3
            };
            this.Add(connect);


            this.locoControl = new Window()
            {
                X = 0,
                Y = 4,
                Width = 31,
                Height = 5,
                Visible = false
            };
            this.locoControl.Add(new Label() { Text = "Speed: ", X = 0, Y = 0 });
            locoSpeed = new TextField
            {
                X = 7,
                Y = 0,
                Width = 4
            };
            this.locoControl.Add(locoSpeed);
            this.reverse = new CheckBox
            {
                Text = "Reverse",
                X = 13,
                Y = 0
            };
            this.locoControl.Add(this.reverse);

            var go = new Button()
            {
                Text = "Set Speed",
                X = 0,
                Y = 1,
            };
            go.Clicked += OnGoClicked;
            this.locoControl.Add(go);

            var stop = new Button()
            {
                Text = "Stop",
                X = 15,
                Y = 1
            };
            stop.Clicked += OnStopClicked;
            this.locoControl.Add(stop);

            this.locoControl.Add(new Label { Text = "Function: ", X = 0, Y = 2 });
            this.locoFunction = new TextField { X = 10, Y = 2, Width = 4 };
            this.locoControl.Add(this.locoFunction);
            var on = new Button
            {
                Text = "On",
                X = 15,
                Y = 2
            };
            on.Clicked += OnOnClicked;
            this.locoControl.Add(on);

            var off = new Button
            {
                Text = "Off",
                X = 22,
                Y = 2
            };
            off.Clicked += OnOffClicked;
            this.locoControl.Add(off);


            this.Add(locoControl);

            connect.Clicked += OnConnectClicked;
        }

        private void OnOffClicked() { 
            if (byte.TryParse(this.locoFunction.Text.ToString(), out var fn))
            {
                engineSession.SetFunction(fn, false);
            }
        }
        private void OnOnClicked()
        {
            if (byte.TryParse(this.locoFunction.Text.ToString(), out var fn))
            {
                engineSession.SetFunction(fn, true);
            }
        }

        private void OnStopClicked()
        {
            SendSpeedDir(0);
        }
        private void OnGoClicked()
        {
            if (byte.TryParse(locoSpeed.Text.ToString(), out var speed))
            {
                //Don't send emergency stop
                if (speed == 1) speed = 2;
                if (!reverse.Checked)
                {
                    speed |= (1<<7);
                }
                SendSpeedDir(speed);
            }
        }

        private void SendSpeedDir(byte speedDir)
        {
            this.engineSession.SpeedDir = speedDir;
        }

        private async void OnConnectClicked()
        {
            //TODO: proper exception handling and logging to prevent exceptions leaving async void event handler
            if (!ushort.TryParse(locoAddress.Text.ToString(), out var loco))
            {
                MessageBox.ErrorQuery("Error", "Please enter a numeric loco address", "Ok");
                return;
            }

            this.engineSession = await engineManager.RequestEngineSession(loco);
            
            this.locoControl.Title = "Address: " + locoAddress.Text;
            this.locoControl.Visible = true;
        }
    }
}