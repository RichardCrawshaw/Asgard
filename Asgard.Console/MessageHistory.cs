using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Communications;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class MessageHistory:Window
    {
        private readonly ICbusMessenger cbusMessenger;

        public MessageHistory(ICbusMessenger cbusMessenger)
        {
            this.cbusMessenger = cbusMessenger;
            this.Title = "Last Message Received";

            var label = new Label
            {
                Text = "",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 1
            };
            this.Add(label);

            cbusMessenger.MessageReceived += (sender, e) =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    var opc = e.Message.GetOpCode();
                    label.Text = $"{opc.Code} {opc}";
                });

            };
        }


    }
}
