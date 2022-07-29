using System;
using Asgard.Communications;
using Asgard.EngineControl;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class ServiceMode : Window
    {
        private readonly EngineManager engineManager;
        private readonly TextField cv;
        private readonly TextField value;
        private IEngineSession session;
        public ServiceMode(ICbusMessenger cbusMessenger)
        {
            this.engineManager = new EngineManager(cbusMessenger);

            this.Add(new Label() { Text = "CV: ", X = 0, Y = 0 });
            cv = new TextField
            {
                X = 8,
                Y = 0,
                Width = 6
            };
            this.Add(cv);

            this.Add(new Label() { Text = "Value: ", X = 0, Y = 1 });
            value = new TextField
            {
                X = 8,
                Y = 1,
                Width = 4
            };
            this.Add(value);

            var read = new Button()
            {
                Text = "Read",
                X = 0,
                Y = 2
            };
            this.Add(read);
            read.Clicked += OnReadClicked;

            var write = new Button()
            {
                Text = "Write",
                X = 9,
                Y = 2
            };
            this.Add(write);
            write.Clicked += OnWriteClicked;

        }

        private async void OnReadClicked()
        {
            if (!ushort.TryParse(cv.Text.ToString(), out var cvIdx))
            {
                MessageBox.ErrorQuery("Error", "Please enter a numeric CV index", "Ok");
                return;
            }
            value.Text = "";
            if (session == null)
            {
                //TODO: make address configurable?
                session = await engineManager.RequestEngineSession(9999, steal: true);
            }
            var val = await engineManager.ServiceModeRead(session, cvIdx, Asgard.Data.ServiceModeEnum.DirectByte);
            value.Text = val.ToString();
        }

        private async void OnWriteClicked()
        {
            if (!ushort.TryParse(cv.Text.ToString(), out var cvIdx))
            {
                MessageBox.ErrorQuery("Error", "Please enter a numeric CV index", "Ok");
                return;
            }
            if (!byte.TryParse(value.Text.ToString(), out var cvValue))
            {
                MessageBox.ErrorQuery("Error", "Please enter a numeric CV value", "Ok");
                return;
            }
            if (session == null)
            {
                //TODO: make address configurable?
                session = await engineManager.RequestEngineSession(9999, steal: true);
            }
            var reply = await engineManager.ServiceModeWrite(session, cvIdx, Asgard.Data.ServiceModeEnum.DirectByte, cvValue);
            if (reply == Asgard.Data.SessionStatusEnum.WriteAck)
            {
                MessageBox.Query("CV Written", "The CV was successfully written", "Ok");
                return;
            } 
            else
            {
                MessageBox.ErrorQuery("Error", $"The CV was not successfully written.  The response was: {reply}.", "Ok");
            }
        }
    }
}
