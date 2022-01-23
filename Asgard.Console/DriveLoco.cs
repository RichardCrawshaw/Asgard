using Asgard.Communications;
using Asgard.EngineControl;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class DriveLoco : Window
    {
        private readonly EngineManager engineManager;

        private readonly TextField locoAddress;
        private LocoSession? locoControl;

        public DriveLoco(ICbusMessenger cbusMessenger)
        {
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


            connect.Clicked += OnConnectClicked;
        }


        private async void OnConnectClicked()
        {
            //TODO: proper exception handling and logging to prevent exceptions leaving async void event handler
            if (!ushort.TryParse(locoAddress.Text.ToString(), out var loco))
            {
                MessageBox.ErrorQuery("Error", "Please enter a numeric loco address", "Ok");
                return;
            }
            //TODO: utilise steal/share flags
            var es = await engineManager.RequestEngineSession(loco);
            this.locoControl = new LocoSession(es)
            {
                X = 0,
                Y = 5
            };
            this.Add(this.locoControl);
        }
    }
}