using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Asgard.Communications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asgard.ExampleGui
{
    public class Controller
    {
        #region Fields

        private readonly MainForm view;
        private readonly ICbusMessenger cbusMessenger;
        private readonly ILogger<Controller> logger;

        private readonly List<string> messages = new();
        private readonly List<string> nodes = new();

        #endregion

        #region Properties

        internal bool MessagesLog { get; set; } = true;

        internal int NodeSort { get; set; }

        #endregion

        #region Constructors

        public Controller(MainForm view,
                          ICbusMessenger cbusMessenger,
                          ILogger<Controller> logger)
        {
            this.view = view;
            this.cbusMessenger = cbusMessenger;
            this.logger = logger;

            this.view.Controller = this;
        }

        #endregion

        #region Public methods

        public void Run()
        {
            this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;

            this.view.FormClosing += View_FormClosing;
            this.view.FormClosed += View_FormClosed;
            this.view.Shown += View_Shown;

            Application.Run(this.view);
        }

        #endregion

        #region Internal methods

        internal void ClearMessages()
        {
            this.messages.Clear();
            DisplayMessages();
        }

        internal void ClearNodes()
        {
            this.nodes.Clear();
            DisplayNodes();
        }

        internal void Connection()
        {
            // TODO: Display a connection dialog.
        }

        internal void RefreshNodes() => DisplayNodes();

        internal async Task StartComms()
        {
            await
                this.cbusMessenger.OpenAsync()
                    .ContinueWith(_ => ConnectionStatus());
        }

        internal void StopComms()
        {
            this.cbusMessenger.Close();
            ConnectionStatus();
        }

        internal void ToggleMessageLogging() => this.MessagesLog = !this.MessagesLog;

        #endregion

        #region Support routines

        private void ConnectionStatus()
        {
            if (this.cbusMessenger.IsOpen)
                this.view.ConnectionStatus("Connected");
            else
                this.view.ConnectionStatus("Disconnected");
        }

        private void DisplayMessages() => this.view.DisplayMessages(this.messages.ToArray());

        private void DisplayNodes()
        {
            var nodes = Math.Sign(this.NodeSort) switch
            {
                1 => this.nodes.OrderBy(n => n).ToList(),
                -1 => this.nodes.OrderByDescending(n => n).ToList(),
                _ => this.nodes,
            };
            this.view.DisplayNodes(nodes.ToArray());
        }

        #endregion

        #region Event handler routines

        private void CbusMessenger_MessageReceived(object? sender, CbusMessageEventArgs e)
        {
            if (this.MessagesLog)
                this.messages.Insert(0, $"{(e.Received ? "<<<" : ">>>")} {e.Message}");
            this.logger?.LogInformation($"Message received: {e.Message.GetOpCode()}");
        }

        private void View_FormClosed(object? sender, FormClosedEventArgs e) =>
            this.cbusMessenger.MessageReceived -= CbusMessenger_MessageReceived;

        private void View_FormClosing(object? sender, FormClosingEventArgs e) =>
            this.cbusMessenger.Close();

        private async void View_Shown(object? sender, EventArgs e) => await StartComms();

        #endregion
    }
}
