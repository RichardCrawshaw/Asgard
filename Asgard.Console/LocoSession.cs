using Asgard.EngineControl;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class LocoSession:Window
    {
        private readonly TextField locoSpeed;
        private readonly TextField locoFunction;
        private readonly CheckBox reverse;
        private readonly IEngineSession engineSession;

        public LocoSession(IEngineSession engineSession)
        {
            this.Width = 31;
            this.Height = 5;

            this.Add(new Label() { Text = "Speed: ", X = 0, Y = 0 });
            locoSpeed = new TextField
            {
                X = 7,
                Y = 0,
                Width = 4
            };
            this.Add(locoSpeed);
            this.reverse = new CheckBox
            {
                Text = "Reverse",
                X = 13,
                Y = 0
            };
            this.Add(this.reverse);

            var go = new Button()
            {
                Text = "Set Speed",
                X = 0,
                Y = 1,
            };
            go.Clicked += OnGoClicked;
            this.Add(go);

            var stop = new Button()
            {
                Text = "Stop",
                X = 15,
                Y = 1
            };
            stop.Clicked += OnStopClicked;
            this.Add(stop);

            this.Add(new Label { Text = "Function: ", X = 0, Y = 2 });
            this.locoFunction = new TextField { X = 10, Y = 2, Width = 4 };
            this.Add(this.locoFunction);
            var on = new Button
            {
                Text = "On",
                X = 15,
                Y = 2
            };
            on.Clicked += OnOnClicked;
            this.Add(on);

            var off = new Button
            {
                Text = "Off",
                X = 22,
                Y = 2
            };
            off.Clicked += OnOffClicked;
            this.Add(off);

            reverse.Checked = engineSession.SpeedDir < 127;
            locoSpeed.Text = (engineSession.SpeedDir % 128).ToString();

            this.Title = "Address: " + engineSession.Address;
            this.engineSession = engineSession;
            this.engineSession.SessionCancelled += OnSessionCancelled;
        }

        private void OnSessionCancelled(object? sender, EventArgs e)
        {
            Application.MainLoop.Invoke(() =>
            {
                //TODO: update UI to reflect engine no longer being under control in this session
            });
        }

        private void OnOffClicked()
        {
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

        private void OnStopClicked() => SendSpeedDir(0);
        private void OnGoClicked()
        {
            if (byte.TryParse(locoSpeed.Text.ToString(), out var speed))
            {
                //Don't send emergency stop
                if (speed == 1) speed = 2;
                if (!reverse.Checked)
                {
                    speed |= (1 << 7);
                }
                SendSpeedDir(speed);
            }
        }

        private void SendSpeedDir(byte speedDir) => this.engineSession.SetSpeedAndDirection(speedDir);
    }
}
