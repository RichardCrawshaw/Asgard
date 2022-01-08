using Asgard.Communications;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class DriveLoco : Window
    {
        private ICbusMessenger cbusMessenger;

        public DriveLoco(ICbusMessenger cbusMessenger)
        {
            this.cbusMessenger = cbusMessenger;

            this.Add(new Label() { Text = "Address: ", X = 0, Y = 0 });

            var address = new TextField()
            {
                Text = "5",
                X = 9,
                Y = 0,
                Width = 5,
            };
            this.Add(address);

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

            connect.Clicked += () =>
            {
                if (!int.TryParse(address.Text.ToString(), out var loco))
                {
                    MessageBox.ErrorQuery("Error", "Please enter a numeric loco address", "Ok");
                    return;
                }
            };
        }
    }
}