using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Asgard.Communications;
using Asgard.Data;
using Asgard.Data.Interfaces;
using Cbus.Gladsheimr.Attributes;
using Cbus.Gladsheimr.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asgard.ExampleGui
{
    public class Controller
    {
        #region Fields

        private readonly static Lazy<List<OpCodeControlInfo>> userControlInfo = 
            new(() => GetOpCodeUserControlTypes());

        private readonly MainForm view;
        private readonly ICbusMessenger cbusMessenger;
        private readonly IOptionsMonitor<CbusModuleOptions> options;
        private readonly ILogger<Controller> logger;

        private readonly MessageManager messageManager;
        private readonly ResponseManager responseManager;
        private readonly EventActionManager eventActionManager;
        private readonly EventStateManager eventStateManager;

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

            this.cbusMessenger.StandardMessageReceived += CbusMessenger_StandardMessageReceived;
            this.cbusMessenger.MessageSent += CbusMessenger_MessageSentAsync;

            this.messageManager = new MessageManager(this.cbusMessenger);

            this.responseManager = new ResponseManager(this.cbusMessenger);
            this.responseManager.Register<QueryNodeNumber>(SendPNNAsync);

            this.eventStateManager = new EventStateManager(this.cbusMessenger, this.logger);

            this.eventActionManager = new EventActionManager(this.cbusMessenger, logger);
            // TODO: register events that this application should respond to.
            this.eventActionManager.RegisterCbusEvent<AccessoryOn>(257, 1, Callback1);
            this.eventActionManager.RegisterCbusEvent<AccessoryOff>(257, 1, m => Callback2(m, null));

            this.view.Controller = this;
        }

        private void Callback1(ICbusAccessoryEvent cbusAccessoryEvent) { }

        private void Callback2(ICbusAccessoryEvent cbusAccessoryEvent, object? data) { }

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
            var menuStrip = new MenuStrip
            {
                Name = "menuStripCompose"
            };

            var menuBarItems =
                new[]
                {
                    "Accessory",
                    "Config",
                    "DCC",
                    "General",
                };

            ToolStripMenuItem Create(OpCodeControlAttribute attribute) => 
                new($"{attribute.Name} ({attribute.Code})", null,
                    ToolStripMenuItem_Compose_Click) { Tag = attribute.Code, };

            foreach (var item in menuBarItems)
            {
                menuStrip.Items.Add(
                    new ToolStripMenuItem($"&{item}", null,
                        item == menuBarItems[0]
                            ? new ToolStripItem[]
                                {
                                    new ToolStripMenuItem("On", null,
                                        userControlInfo.Value
                                            .Where(n => n.IsAccessoryOn)
                                            .Select(n => Create(n.Attribute))
                                            .OrderBy(n => n.Text)
                                            .ToArray()),
                                    new ToolStripMenuItem("Off", null,
                                        userControlInfo.Value
                                            .Where(n => n.IsAccessoryOff)
                                            .Select(n => Create(n.Attribute))
                                            .OrderBy(n => n.Text)
                                            .ToArray()),
                                    new ToolStripSeparator(),
                                }.Union(
                                    userControlInfo.Value
                                        .Where(n => n.Attribute.Group?.Equals(item) ?? false)
                                        .Where(n => !n.IsAccessoryOn)
                                        .Where(n => !n.IsAccessoryOff)
                                        .Select(n => Create(n.Attribute))
                                        .OrderBy(n => n.Text))
                                .ToArray()
                            : userControlInfo.Value
                                .Where(n => n.Attribute.Group?.Equals(item) ?? false)
                                .Select(n => Create(n.Attribute))
                                .OrderBy(n => n.Text)
                                .ToArray())
                    {
                        Name = $"tsmi{item}",
                    });
            }

            menuStrip.Items.Add(new ToolStripMenuItem("Clear", null, ToolStripMenuItem_Clear_Click));

            view.AddControls(menuStrip);
        }

        internal async void RefreshNodes()
        {
            try
            {
                await
                        this.messageManager.SendMessageWaitForReplies<ResponseToQueryNode>(new QueryNodeNumber());
            }
            catch (TimeoutException ex)
            {
                this.logger?.LogWarning(ex, $"{nameof(RefreshNodes)} timed out.");
            }
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

        private static List<OpCodeControlInfo> GetOpCodeUserControlTypes()
        {
            var results = new List<OpCodeControlInfo>();

            var assembly = Assembly.GetAssembly(typeof(OpCodeControlAttribute));
            var info =
                assembly?.GetTypes()
                    .Select(t => OpCodeControlInfo.Create(t))
                    .ToList();
            if (info is null ) return results;

            foreach (var item in info)
                if (item is not null)
                    results.Add(item);
            return results;
        }

        private async Task HandleMessage(ICbusOpCode opcode)
        {
            await
                Task.Run(() =>
                {
                    if (opcode is IHasNodeNumber hasNodeNumber)
                        AddNode(hasNodeNumber.NodeNumber);
                });
        }

        private void LogMessage(ICbusMessage? message, bool isReceived)
        {
            if (this.MessagesLog)
                this.messages.Insert(0, $"{(isReceived ? "<<<" : ">>>")} {message}");
        }

        private async Task SendMessage(string opCode, UserControl control)
        {
            // Get a list of the properties available from the control.
            // Get the op-code to send.
            // Get a list of the matching properties to be set on the op-code.
            // Copy the property values from the control to the op-code.
            // Send a message containing the op-code.

            var controlType = control.GetType();
            var controlProperties =
                controlType.GetProperties()
                    .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<OpCodeParameterAttribute>(), })
                    .Where(n => n.Attribute is not null)
                    .Where(n => n.Property.CanRead)
                    .ToList();

            var opCodeObject = OpCodeData.Create(opCode);
            if (opCodeObject is null) return;

            var opCodeType = opCodeObject.GetType();
            var opCodeProperties =
                opCodeType.GetProperties()
                    .Where(p => controlProperties.Any(cp => cp.Attribute?.PropertyName?.Equals(p.Name) ?? false))
                    .Where(p => p.CanWrite)
                    .ToList();

            foreach(var controlProperty in controlProperties)
            {
                var opCodeProperty =
                    opCodeProperties
                        .First(p => p.Name.Equals(controlProperty.Attribute?.PropertyName));
                var value = controlProperty.Property.GetValue(control, null);
                opCodeProperty.SetValue(opCodeObject, value);
            }

            await
                this.cbusMessenger.SendMessage(opCodeObject);
        }

        #endregion

        #region OpCode send routines

        private async Task SendPNNAsync(ICbusMessenger cbusMessenger, ICbusMessage cbusMessage, QueryNodeNumber msg)
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

        #endregion

        #region Event handler routines

        private async void CbusMessenger_StandardMessageReceived(object? sender, CbusStandardMessageEventArgs e)
        {
            LogMessage(e.Message, e.Received);

            if (e.Message.TryGetOpCode(out var opCode))
            {
                this.logger?.LogInformation("Message received: {opCode}", opCode);
                await DisplayMessages();
                await HandleMessage(opCode);
            }
            else
                this.logger?.LogInformation("Unknown message received: {m}", e.Message?.ToString() ?? "null");
        }

        private async void CbusMessenger_MessageSentAsync(object? sender, CbusMessageEventArgs e)
        {
            LogMessage(e.Message, e.Received);
            await DisplayMessages();
        }

        private void ToolStripMenuItem_Clear_Click(object? sender, EventArgs e) =>
            this.view.ClearControls(includeMenu: false);

        private void ToolStripMenuItem_Compose_Click(object? sender, EventArgs e)
        {
            // Determine which op-code control relates to the selected menu item.
            // Create an instance of it, together with a label to add a caption.
            // Arrange them both so they can be drawn on screen in the correct relationship.
            // Clear any existing controls and add them to the form.

            if (sender is null) return;
            if (sender is not ToolStripMenuItem tsmi) return;
            var tag = tsmi.Tag as string;
            if (string.IsNullOrEmpty(tag)) return;

            var userControlItem =
                Controller.userControlInfo.Value
                    .FirstOrDefault(n => n.Attribute.Code?.Equals(tag) ?? false);
            if (userControlItem is null) return;

            var userControl = userControlItem.Instantiate();
            if (userControl is null) return;

            var label = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(3, 30),
                Text = userControlItem.Attribute.Name,
            };

            userControl.AutoSize = true;
            userControl.BorderStyle = BorderStyle.None;
            userControl.Location = new System.Drawing.Point(3, label.Top + label.Height + 6);
            userControl.Tag = label;
            if (userControl is not IOpCodeControl opCodeControl) return;
            opCodeControl.Actioned += OpCodeControl_Actioned;

            this.view.ClearControls(includeMenu: false);
            this.view.AddControls(
                label,
                userControl);
        }

        private async void OpCodeControl_Actioned(object? sender, EventArgs e)
        {
            // Attempts to action the current op-code control.
            // Remove this event handler routine.
            // Get the code of the op-code.
            // Construct a list of the control that are to be removed.
            // Send the message using the data from the op-code control.
            // Remove the controls and clear any text.

            if (sender is not IOpCodeControl opCodeControl) return;
            if (sender is not UserControl control) return;

            opCodeControl.Actioned -= OpCodeControl_Actioned;
            var opCode = opCodeControl.OpCodeCode;

            var controls = new List<Control> { control };
            if (control.Tag is Control label)
                controls.Add(label);

            await SendMessage(opCode, control);

            this.view.RemoveControls(controls.ToArray());
            this.view.ClearText();
        }

        private void View_FormClosed(object? sender, FormClosedEventArgs e) =>
            this.cbusMessenger.StandardMessageReceived -= CbusMessenger_StandardMessageReceived;

        private void View_FormClosing(object? sender, FormClosingEventArgs e) =>
            this.cbusMessenger.Close();

        private async void View_Shown(object? sender, EventArgs e) => await StartComms();

        #endregion

        #region Nested classes

        private class OpCodeControlInfo
        {
            public Type Type { get; }
            public OpCodeControlAttribute Attribute { get; }
            public bool IsAccessoryOn =>
                this.Attribute.Group?.Equals("Accessory", StringComparison.OrdinalIgnoreCase) ?? false &&
                ((this.Attribute.Name?.Contains(" on ", StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (this.Attribute.Name?.EndsWith(" on", StringComparison.OrdinalIgnoreCase) ?? false));
            public bool IsAccessoryOff =>
                this.Attribute.Group?.Equals("Accessory", StringComparison.OrdinalIgnoreCase) ?? false &&
                ((this.Attribute.Name?.Contains(" off ", StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (this.Attribute.Name?.EndsWith(" off", StringComparison.OrdinalIgnoreCase) ?? false ));

            private OpCodeControlInfo(Type type, OpCodeControlAttribute attribute)
            {
                this.Type = type;
                this.Attribute = attribute;
            }

            public static OpCodeControlInfo? Create(Type type)
            {
                if (type is null) return null;
                var attribute = type.GetCustomAttribute<OpCodeControlAttribute>();
                if (attribute is null) return null;

                return new OpCodeControlInfo(type, attribute);
            }

            public UserControl? Instantiate()
            {
                var constructor =
                    this.Type.GetConstructor(Array.Empty<Type>());
                if (constructor is null) return null;

                var userControl = (UserControl)
                    constructor.Invoke(Array.Empty<object>());
                return userControl;
            }
        }

        #endregion
    }
}
