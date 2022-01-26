using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Asgard.Communications;
using Asgard.Data;
using Cbus.Gladsheimr.Attributes;
using Cbus.Gladsheimr.Interfaces;
using Cbus.Gladsheimr.UserControls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asgard.ExampleGui
{
    public class Controller
    {
        #region Fields

        private readonly MainForm view;
        private readonly ICbusMessenger cbusMessenger;
        private readonly IOptionsMonitor<CbusModuleOptions> options;
        private readonly ILogger<Controller> logger;
        private readonly MessageManager messageManager;
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
                          IOptionsMonitor<CbusModuleOptions> options,
                          ILogger<Controller> logger)
        {
            this.view = view;
            this.cbusMessenger = cbusMessenger;
            this.options = options;
            this.logger = logger;

            this.messageManager = new MessageManager(this.cbusMessenger);

            this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;
            this.cbusMessenger.MessageSent += CbusMessenger_MessageSentAsync;

            var rm = new ResponseManager(this.cbusMessenger);
            rm.Register<QueryNodeNumber>(SendPNNAsync);

            this.view.Controller = this;
        }

        #endregion

        #region Public methods

        public void Run()
        {
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
            _ = DisplayMessages();
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

        internal void DisplayComposeMenu()
        {
            const string name = "menuStripCompose";

            var menuStrip = new MenuStrip
            {
                Name = name
            };
            menuStrip.Items.Add(new ToolStripMenuItem("&Accessories"));
            menuStrip.Items.Add(new ToolStripMenuItem("&Config", null,
                new ToolStripMenuItem("&Query Node Number (QNN)",
                    null, ToolStripMenuItem_Compose_Click) {Tag = "QNN", },
                new ToolStripMenuItem("&Query Node Number Response (PNN)",
                    null, ToolStripMenuItem_Compose_Click) { Tag = "PNN", }
            ));
            menuStrip.Items.Add(new ToolStripMenuItem("&DCC"));
            menuStrip.Items.Add(new ToolStripMenuItem("&General"));

            view.AddControls(menuStrip);
        }

        internal async void RefreshNodes()
        {
            await
                this.messageManager.SendMessageWaitForReplies<ResponseToQueryNode>(new QueryNodeNumber());
            DisplayNodes();
        }

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

        private void AddNode(ushort nodeNumber)
        {
            var nodeText = $"NN: {nodeNumber}";
            if (this.nodes.Contains(nodeText))
                return;
            this.nodes.Add(nodeText);
            DisplayNodes();
        }

        private void ConnectionStatus()
        {
            if (this.cbusMessenger.IsOpen)
                this.view.ConnectionStatus("Connected");
            else
                this.view.ConnectionStatus("Disconnected");
        }

        private async Task DisplayMessages() => await Task.Run(() => this.view.DisplayMessages(this.messages.ToArray()));

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

        private async Task HandleMessage(ICbusMessage message) => await HandleMessage(message.GetOpCode());

        private async Task HandleMessage(ICbusOpCode opcode)
        {
            if (opcode is IHasNodeNumber hasNodeNumber)
                AddNode(hasNodeNumber.NodeNumber);
            if (opcode is IQueryNodeNumber)
                await SendPNNAsync();
        }

        private void LogMessage(ICbusMessage message, bool isReceived)
        {
            if (this.MessagesLog)
                this.messages.Insert(0, $"{(isReceived ? "<<<" : ">>>")} {message}");
        }

        private async Task SendOpCode(string opCode, params Control[] controls)
        {
            this.view.RemoveControls(controls);
            this.view.ClearText();

            await Task.Run(() =>
            {
                var routine =
                    typeof(Controller).GetMethods()
                        .Select(m => new
                        {
                            Method = m,
                            Attribute = m.GetCustomAttribute<OpCodeAttribute>(),
                        })
                        .FirstOrDefault(n => n.Attribute?.Code.Equals(opCode) ?? false);
                if (routine is not null) 
                    routine.Method.Invoke(this, controls);
            });
        }

        #endregion

        #region OpCode send routines

        // All these routines are invoked through reflection.
        // To send a message create one of these routines that creates the message that is to be 
        // sent; decorate it with the op-code that it sends. Then add a menu item with that same
        // op-code in the tag.
        // Selecting the menu item will display the UserControl for the op-code; clicking on the
        // Send button of the UserControl will cause the matching Send routine to be run.

        [OpCode(Code = "PNN")]
        private async Task SendPNNAsync() => await SendPNNAsync(this.cbusMessenger, null);

        private async Task SendPNNAsync(ICbusMessenger cbusMessenger, ICbusMessage? cbusMessage)
        {
            await
                cbusMessenger.SendMessage(
                    new ResponseToQueryNode
                    {
                        ManufId = this.options.CurrentValue.ManufacturerId,
                        ModuleId = this.options.CurrentValue.ModuleId,
                        NodeFlags = NodeFlagsEnum.Consumer |
                                    NodeFlagsEnum.Producer |
                                    NodeFlagsEnum.FLiMMode,
                        NodeNumber = this.options.CurrentValue.NodeNumber,
                    });
        }

        [OpCode(Code = "QNN")]
        private async Task SendQNNAsync()
        {
            var message = new QueryNodeNumber();
            LogMessage(message.Message, false);
            var replies = await
                this.messageManager.SendMessageWaitForReplies<ResponseToQueryNode>(message);
            foreach (var reply in replies)
                this.logger.LogInformation(
                    $"Node info: Node number: {reply.NodeNumber}, ModuleID: {reply.ModuleId}");
            if (replies.Any())
                this.view.DisplayText(
                    replies
                        .Select(n => $"NN: {n.NodeNumber} ID: {n.ModuleId}")
                        .ToArray());
        }

        #endregion

        #region Event handler routines

        private async void CbusMessenger_MessageReceived(object? sender, CbusMessageEventArgs e)
        {
            LogMessage(e.Message, e.Received);
            this.logger?.LogInformation($"Message received: {e.Message.GetOpCode()}");
            await DisplayMessages();
            await HandleMessage(e.Message);
        }

        private async void CbusMessenger_MessageSentAsync(object? sender, CbusMessageEventArgs e)
        {
            LogMessage(e.Message, e.Received);
            await DisplayMessages();
        }

        private void ToolStripMenuItem_Compose_Click(object? sender, EventArgs e)
        {
            if (sender is null) return;
            if (sender is not ToolStripMenuItem tsmi) return;
            var tag = tsmi.Tag as string;
            if (string.IsNullOrEmpty(tag)) return;

            // Find an opcode control with the code contained in the tag.
            var assembly = Assembly.GetAssembly(typeof(OpCodeControlAttribute));
            var userControlInfo =
                assembly?.GetTypes()
                    .Select(t => new
                    {
                        Type = t,
                        Attribute = t.GetCustomAttribute<OpCodeControlAttribute>(true),
                    })
                    .FirstOrDefault(n => n.Attribute?.Code.Equals(tag) ?? false);
            if (userControlInfo is null) return;

            var constructor =
                userControlInfo.Type.GetConstructor(Type.EmptyTypes);
            if (constructor is null) return;

            var userControl = (UserControl)constructor.Invoke(Array.Empty<object>());
            if (userControl is null) return;

            userControl.AutoSize = true;
            userControl.Location = new System.Drawing.Point(3, 28);
            if (userControl is not IOpCodeControl opCodeControl) return;
            opCodeControl.Actioned += OpCodeControl_Actioned;

            this.view.AddControls(userControl);
        }

        private async void OpCodeControl_Actioned(object? sender, EventArgs e)
        {
            if (sender is not IOpCodeControl opCodeControl) return;
            opCodeControl.Actioned -= OpCodeControl_Actioned;
            if (sender is not Control control) return;
            var opCode = opCodeControl.OpCodeCode;
            await SendOpCode(opCode, control);
        }

        private void View_FormClosed(object? sender, FormClosedEventArgs e) =>
            this.cbusMessenger.MessageReceived -= CbusMessenger_MessageReceived;

        private void View_FormClosing(object? sender, FormClosingEventArgs e) =>
            this.cbusMessenger.Close();

        private async void View_Shown(object? sender, EventArgs e) => await StartComms();

        #endregion
    }
}
