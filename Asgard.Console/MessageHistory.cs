using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
using Terminal.Gui;
using Terminal.Gui.Views;

namespace Asgard.Console
{
    internal class MessageHistory:Window
    {
        private readonly List<string> history = new();
        public MessageHistory(ICbusMessenger cbusMessenger)
        {
            this.Border = new Border() { BorderThickness = new Thickness(0) };

            var line = new LineView()
            {
                Y = 0
            };
            this.Add(line);
            this.Add(new Label()
            {
                Text = "Last Message Received",
                X = 2,
                Y = 0,
                AutoSize = true,
            });
            var label = new Label
            {
                Text = "",
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };
            this.Add(label);

            var button = new Button("History")
            {
                X = Pos.Right(this) - 11,
                Y = 0
            };
            this.Add(button);
            button.Clicked += ShowHistory;

            cbusMessenger.StandardMessageReceived += (sender, e) =>
              {
                  string? message = null;

                  if (e.Message?.TryGetOpCode(out var opCode) ?? false)
                      message = opCode.ToString();
                  history.Add(message ?? "Unknown message");

                  while (history.Count > 20)
                      history.RemoveAt(0);

                  Application.MainLoop.Invoke(() => label.Text = message);
              };
        }

        private void ShowHistory()
        {
            var d = new Dialog()
            {
                Title = "Last 20 messages",
            };
            var l = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()-1
            };
            l.SetSource(history);
            d.Add(l);

            l.AddScrollbar();

            var b = new Button("Close", true);
            b.Clicked += () => Application.RequestStop(d);
            d.AddButton(b);

            Application.Run(d);

        }
    }
}
