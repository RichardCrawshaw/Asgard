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

        internal void ConnectionStatus(string status) => this.tsslConnectionInfo.Text = status;

        internal void DisplayMessages(string[] values) => Display(this.panelMessages, 13f, 5f, 20f, values);

        internal void DisplayNodes(string[] values) => Display(this.panelNodes, 13f, 5f, 20f, values);

        #endregion

        #region Support routines

        private static void Display(Control control, float x, float y, float v, string[] values)
        {
            void Display(object sender, PaintEventArgs e) =>
                MainForm.Display(e.Graphics, control, x, y, v, values);

            control.Paint += Display;
            control.Refresh();
            control.Paint -= Display;
        }

        private static void Display(Graphics graphics, Control control, float x, float y, float vDelta, string[] values)
        {
            graphics.FillRectangle(new SolidBrush(control.BackColor), control.Bounds);

            var font = control.Font;
            var brush = new Pen(control.ForeColor).Brush;
            foreach (var value in values)
                graphics.DrawString(value, font, brush, x, y += vDelta);
        }

        #endregion

        #region Event handler routines

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

        private void TsmiMessagesCompose_Click(object sender, EventArgs e)
        {

        }

        private void TsmiCommsStart_Click(object sender, EventArgs e) => this.Controller?.StartComms();

        private void TsmiCommsStop_Click(object sender, EventArgs e) => this.Controller?.StopComms();

        private void TsmiCommsConnection_Click(object sender, EventArgs e) => this.Controller?.Connection();

        #endregion
    }
}
