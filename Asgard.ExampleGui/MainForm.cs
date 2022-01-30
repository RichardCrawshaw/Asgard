using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Asgard.ExampleGui
{
    public partial class MainForm : Form
    {
        #region Properties

        internal Controller? Controller { get; set; }

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        internal void AddControls(params Control[] controls)
        {
            // Remove any existing controls that have a matching name with a new one.
            // Empty or null names don't get removed.
            controls
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .Select(c => GetNamedControl(this.panelMain.Controls, c.Name))
                .Where(n => n is not null)
                .ToList()
                .ForEach(n => this.panelMain.Controls.Remove(n));

            // Clear any displayed graphics on the background.
            this.panelMain.Refresh();

            this.panelMain.Controls.AddRange(controls.ToArray());

            this.panelMain.SuspendLayout();
            foreach (Control control in this.panelMain.Controls)
            {
                if (control is MenuStrip) continue;
                control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                control.Size = new Size(control.Parent.Width - 3 * control.Left, control.Height);
            }
            this.panelMain.ResumeLayout();
        }

        internal void ClearControls(bool includeMenu = true)
        {
            if (includeMenu)
                this.panelMain.Controls.Clear();
            else
            {
                var controls = new List<Control>();
                foreach (Control control in this.panelMain.Controls)
                    controls.Add(control);
                foreach (var control in controls)
                    if (control is not ToolStrip)
                        this.panelMain.Controls.Remove(control);
            }
        }

        internal void ClearText()
        {
            void refresh(object sender, PaintEventArgs e)
            {
                var graphics = e.Graphics;
                graphics.Clear(this.panelMain.BackColor);

                this.panelMain.Paint -= refresh;
            }

            this.panelMain.Paint += refresh;
            this.panelMain.Refresh();
        }

        internal void ConnectionStatus(string status) => this.tsslConnectionInfo.Text = status;

        internal void DisplayMessages(string[] values) => Display(this.panelMessages, 13f, 5f, 20f, values);

        internal void DisplayNodes(string[] values) => Display(this.panelNodes, 13f, 5f, 20f, values);

        internal void DisplayText(params string[] text)
        {
            void refresh(object sender, PaintEventArgs e)
            {
                var graphics = e.Graphics;
                graphics.Clear(this.panelMain.BackColor);
                var brush = new Pen(Color.Black).Brush;
                var y = 8f;
                var v = 20f;
                foreach(var item in text)
                    graphics.DrawString(item, this.panelMain.Font, brush, 3f, y += v);

                this.panelMain.Paint -= refresh;
            }

            this.panelMain.Paint += refresh;
            this.panelMain.Refresh();
        }

        internal void RemoveControls(params Control?[] controls) =>
            controls
                .Where(c => c is not null)
                .ToList()
                .ForEach(c => this.panelMain.Controls.Remove(c));

        #endregion

        #region Support routines

        private void Display(Control control, float x, float y, float v, string[] values)
        {
            void Display(object sender, PaintEventArgs e) =>
                MainForm.Display(e.Graphics, control, x, y, v, values);

            Invoke((MethodInvoker)delegate
            {
                control.Paint += Display;
                control.Refresh();
                control.Paint -= Display;
            });
        }

        private static void Display(Graphics graphics, Control control, float x, float y, float vDelta, string[] values)
        {
            graphics.FillRectangle(new SolidBrush(control.BackColor), control.Bounds);

            var font = control.Font;
            var brush = new Pen(control.ForeColor).Brush;
            foreach (var value in values)
                graphics.DrawString(value, font, brush, x, y += vDelta);
        }

        private static Control? GetNamedControl(Control.ControlCollection controlCollection, string name)
        {
            foreach (Control control in controlCollection)
                if (control.Name == name)
                    return control;
            return null;
        }

        #endregion

        #region Event handler routines

        private void PictureBoxNodes_Click(object sender, EventArgs e) => this.Controller?.RefreshNodes();

        private void TsmiFileExit_Click(object sender, EventArgs e) => Close();

        private void TsmiNodesClear_Click(object sender, EventArgs e) => this.Controller?.ClearNodes();

        private void TsmiNodesRefresh_Click(object sender, EventArgs e) => this.Controller?.RefreshNodes();

        private void TsmiNodesSort_Click(object sender, EventArgs e)
        {
            if (this.Controller is null) return;
            if (sender is not ToolStripMenuItem tsmi) return;
            if (tsmi.Tag is not string value) return;
            if (!int.TryParse(value, out var index)) return;

            var menuItems =
                new[]
                {
                        this.tsmiNodesSortUnsorted,
                        this.tsmiNodesSortAscending,
                        this.tsmiNodesSortDescending,
                };
            foreach (var item in menuItems)
                item.Checked = item.Tag.Equals(index.ToString());
            this.Controller.NodeSort = index;
            this.Controller.RefreshNodes();
        }

        private void TsmiMessagesLog_Click(object sender, EventArgs e) => this.Controller?.ToggleMessageLogging();

        private void TsmiMessagesClear_Click(object sender, EventArgs e) => this.Controller?.ClearMessages();

        private void TsmiMessagesCompose_Click(object sender, EventArgs e) => this.Controller?.DisplayComposeMenu();

        private void TsmiCommsStart_Click(object sender, EventArgs e) => this.Controller?.StartComms();

        private void TsmiCommsStop_Click(object sender, EventArgs e) => this.Controller?.StopComms();

        private void TsmiCommsConnection_Click(object sender, EventArgs e) => this.Controller?.Connection();

        #endregion
    }
}
