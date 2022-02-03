namespace Asgard.ExampleGui
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNodes = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNodesClear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNodesRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tssNodes1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiNodesSort = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNodesSortUnsorted = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiNodesSortAscending = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNodesSortDescending = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMessages = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMessagesLog = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMessagesClear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMessagesCompose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiComms = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCommsStart = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCommsStop = new System.Windows.Forms.ToolStripMenuItem();
            this.tssComms1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiCommsConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslConnectionInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panelMessages = new System.Windows.Forms.Panel();
            this.panelNodes = new System.Windows.Forms.Panel();
            this.pictureBoxNodes = new System.Windows.Forms.PictureBox();
            this.splitterNodes = new System.Windows.Forms.Splitter();
            this.splitterMessages = new System.Windows.Forms.Splitter();
            this.panelMain = new System.Windows.Forms.Panel();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panelMessages.SuspendLayout();
            this.panelNodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNodes)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 3);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(52, 20);
            label1.TabIndex = 0;
            label1.Text = "Nodes";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 3);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(73, 20);
            label2.TabIndex = 0;
            label2.Text = "Messages";
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.tsmiNodes,
            this.tsmiMessages,
            this.tsmiComms});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(6, 3, 0, 3);
            this.menuStrip.Size = new System.Drawing.Size(800, 30);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // tsmiFile
            // 
            this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFileExit});
            this.tsmiFile.Name = "tsmiFile";
            this.tsmiFile.Size = new System.Drawing.Size(46, 24);
            this.tsmiFile.Text = "&File";
            // 
            // tsmiFileExit
            // 
            this.tsmiFileExit.Name = "tsmiFileExit";
            this.tsmiFileExit.Size = new System.Drawing.Size(116, 26);
            this.tsmiFileExit.Text = "E&xit";
            this.tsmiFileExit.Click += new System.EventHandler(this.TsmiFileExit_Click);
            // 
            // tsmiNodes
            // 
            this.tsmiNodes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNodesClear,
            this.tsmiNodesRefresh,
            this.tssNodes1,
            this.tsmiNodesSort});
            this.tsmiNodes.Name = "tsmiNodes";
            this.tsmiNodes.Size = new System.Drawing.Size(66, 24);
            this.tsmiNodes.Text = "&Nodes";
            // 
            // tsmiNodesClear
            // 
            this.tsmiNodesClear.Name = "tsmiNodesClear";
            this.tsmiNodesClear.Size = new System.Drawing.Size(141, 26);
            this.tsmiNodesClear.Text = "&Clear";
            this.tsmiNodesClear.Click += new System.EventHandler(this.TsmiNodesClear_Click);
            // 
            // tsmiNodesRefresh
            // 
            this.tsmiNodesRefresh.Name = "tsmiNodesRefresh";
            this.tsmiNodesRefresh.Size = new System.Drawing.Size(141, 26);
            this.tsmiNodesRefresh.Text = "&Refresh";
            this.tsmiNodesRefresh.Click += new System.EventHandler(this.TsmiNodesRefresh_Click);
            // 
            // tssNodes1
            // 
            this.tssNodes1.Name = "tssNodes1";
            this.tssNodes1.Size = new System.Drawing.Size(138, 6);
            // 
            // tsmiNodesSort
            // 
            this.tsmiNodesSort.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNodesSortUnsorted,
            this.toolStripSeparator2,
            this.tsmiNodesSortAscending,
            this.tsmiNodesSortDescending});
            this.tsmiNodesSort.Name = "tsmiNodesSort";
            this.tsmiNodesSort.Size = new System.Drawing.Size(141, 26);
            this.tsmiNodesSort.Text = "&Sort";
            // 
            // tsmiNodesSortUnsorted
            // 
            this.tsmiNodesSortUnsorted.Checked = true;
            this.tsmiNodesSortUnsorted.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiNodesSortUnsorted.Name = "tsmiNodesSortUnsorted";
            this.tsmiNodesSortUnsorted.Size = new System.Drawing.Size(170, 26);
            this.tsmiNodesSortUnsorted.Tag = "0";
            this.tsmiNodesSortUnsorted.Text = "&Unsorted";
            this.tsmiNodesSortUnsorted.ToolTipText = "The nodes will be shown in the order that they have been discovered";
            this.tsmiNodesSortUnsorted.Click += new System.EventHandler(this.TsmiNodesSort_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(167, 6);
            // 
            // tsmiNodesSortAscending
            // 
            this.tsmiNodesSortAscending.Name = "tsmiNodesSortAscending";
            this.tsmiNodesSortAscending.Size = new System.Drawing.Size(170, 26);
            this.tsmiNodesSortAscending.Tag = "1";
            this.tsmiNodesSortAscending.Text = "&Ascending";
            this.tsmiNodesSortAscending.ToolTipText = "The nodes will be shown in alphabetical order";
            this.tsmiNodesSortAscending.Click += new System.EventHandler(this.TsmiNodesSort_Click);
            // 
            // tsmiNodesSortDescending
            // 
            this.tsmiNodesSortDescending.Name = "tsmiNodesSortDescending";
            this.tsmiNodesSortDescending.Size = new System.Drawing.Size(170, 26);
            this.tsmiNodesSortDescending.Tag = "-1";
            this.tsmiNodesSortDescending.Text = "&Descending";
            this.tsmiNodesSortDescending.ToolTipText = "The nodes will be shown in reverse alphabetical order";
            this.tsmiNodesSortDescending.Click += new System.EventHandler(this.TsmiNodesSort_Click);
            // 
            // tsmiMessages
            // 
            this.tsmiMessages.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiMessagesLog,
            this.tsmiMessagesClear,
            this.tsmiMessagesCompose});
            this.tsmiMessages.Name = "tsmiMessages";
            this.tsmiMessages.Size = new System.Drawing.Size(87, 24);
            this.tsmiMessages.Text = "&Messages";
            // 
            // tsmiMessagesLog
            // 
            this.tsmiMessagesLog.Checked = true;
            this.tsmiMessagesLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiMessagesLog.Name = "tsmiMessagesLog";
            this.tsmiMessagesLog.Size = new System.Drawing.Size(155, 26);
            this.tsmiMessagesLog.Text = "&Log";
            this.tsmiMessagesLog.Click += new System.EventHandler(this.TsmiMessagesLog_Click);
            // 
            // tsmiMessagesClear
            // 
            this.tsmiMessagesClear.Name = "tsmiMessagesClear";
            this.tsmiMessagesClear.Size = new System.Drawing.Size(155, 26);
            this.tsmiMessagesClear.Text = "&Clear";
            this.tsmiMessagesClear.Click += new System.EventHandler(this.TsmiMessagesClear_Click);
            // 
            // tsmiMessagesCompose
            // 
            this.tsmiMessagesCompose.Name = "tsmiMessagesCompose";
            this.tsmiMessagesCompose.Size = new System.Drawing.Size(155, 26);
            this.tsmiMessagesCompose.Text = "Co&mpose";
            this.tsmiMessagesCompose.Click += new System.EventHandler(this.TsmiMessagesCompose_Click);
            // 
            // tsmiComms
            // 
            this.tsmiComms.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCommsStart,
            this.tsmiCommsStop,
            this.tssComms1,
            this.tsmiCommsConnection});
            this.tsmiComms.Name = "tsmiComms";
            this.tsmiComms.Size = new System.Drawing.Size(73, 24);
            this.tsmiComms.Text = "&Comms";
            // 
            // tsmiCommsStart
            // 
            this.tsmiCommsStart.Name = "tsmiCommsStart";
            this.tsmiCommsStart.Size = new System.Drawing.Size(167, 26);
            this.tsmiCommsStart.Text = "&Start";
            this.tsmiCommsStart.Click += new System.EventHandler(this.TsmiCommsStart_Click);
            // 
            // tsmiCommsStop
            // 
            this.tsmiCommsStop.Name = "tsmiCommsStop";
            this.tsmiCommsStop.Size = new System.Drawing.Size(167, 26);
            this.tsmiCommsStop.Text = "&Stop";
            this.tsmiCommsStop.Click += new System.EventHandler(this.TsmiCommsStop_Click);
            // 
            // tssComms1
            // 
            this.tssComms1.Name = "tssComms1";
            this.tssComms1.Size = new System.Drawing.Size(164, 6);
            // 
            // tsmiCommsConnection
            // 
            this.tsmiCommsConnection.Name = "tsmiCommsConnection";
            this.tsmiCommsConnection.Size = new System.Drawing.Size(167, 26);
            this.tsmiCommsConnection.Text = "&Connection";
            this.tsmiCommsConnection.Click += new System.EventHandler(this.TsmiCommsConnection_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslConnection,
            this.tsslConnectionInfo});
            this.statusStrip.Location = new System.Drawing.Point(0, 425);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 26);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // tsslConnection
            // 
            this.tsslConnection.Name = "tsslConnection";
            this.tsslConnection.Size = new System.Drawing.Size(87, 20);
            this.tsslConnection.Text = "Connection:";
            // 
            // tsslConnectionInfo
            // 
            this.tsslConnectionInfo.Name = "tsslConnectionInfo";
            this.tsslConnectionInfo.Size = new System.Drawing.Size(68, 20);
            this.tsslConnectionInfo.Text = "unknown";
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // panelMessages
            // 
            this.panelMessages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMessages.Controls.Add(label2);
            this.panelMessages.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelMessages.Location = new System.Drawing.Point(503, 30);
            this.panelMessages.Name = "panelMessages";
            this.panelMessages.Size = new System.Drawing.Size(297, 395);
            this.panelMessages.TabIndex = 4;
            // 
            // panelNodes
            // 
            this.panelNodes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelNodes.Controls.Add(this.pictureBoxNodes);
            this.panelNodes.Controls.Add(label1);
            this.panelNodes.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelNodes.Location = new System.Drawing.Point(0, 30);
            this.panelNodes.Name = "panelNodes";
            this.panelNodes.Size = new System.Drawing.Size(139, 395);
            this.panelNodes.TabIndex = 6;
            // 
            // pictureBoxNodes
            // 
            this.pictureBoxNodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxNodes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxNodes.BackgroundImage")));
            this.pictureBoxNodes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxNodes.Location = new System.Drawing.Point(107, -1);
            this.pictureBoxNodes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBoxNodes.Name = "pictureBoxNodes";
            this.pictureBoxNodes.Size = new System.Drawing.Size(24, 24);
            this.pictureBoxNodes.TabIndex = 1;
            this.pictureBoxNodes.TabStop = false;
            this.pictureBoxNodes.Click += new System.EventHandler(this.PictureBoxNodes_Click);
            // 
            // splitterNodes
            // 
            this.splitterNodes.Location = new System.Drawing.Point(139, 30);
            this.splitterNodes.Name = "splitterNodes";
            this.splitterNodes.Size = new System.Drawing.Size(5, 395);
            this.splitterNodes.TabIndex = 8;
            this.splitterNodes.TabStop = false;
            // 
            // splitterMessages
            // 
            this.splitterMessages.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitterMessages.Location = new System.Drawing.Point(498, 30);
            this.splitterMessages.Name = "splitterMessages";
            this.splitterMessages.Size = new System.Drawing.Size(5, 395);
            this.splitterMessages.TabIndex = 9;
            this.splitterMessages.TabStop = false;
            // 
            // panelMain
            // 
            this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(144, 30);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(354, 395);
            this.panelMain.TabIndex = 10;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "refresh.ico");
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 451);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.splitterMessages);
            this.Controls.Add(this.splitterNodes);
            this.Controls.Add(this.panelNodes);
            this.Controls.Add(this.panelMessages);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Asgard Example GUI";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelMessages.ResumeLayout(false);
            this.panelMessages.PerformLayout();
            this.panelNodes.ResumeLayout(false);
            this.panelNodes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNodes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsmiFile;
        private System.Windows.Forms.ToolStripMenuItem tsmiFileExit;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.Panel panelMessages;
        private System.Windows.Forms.Panel panelNodes;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodes;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodesClear;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodesRefresh;
        private System.Windows.Forms.ToolStripSeparator tssNodes1;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodesSort;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodesSortUnsorted;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodesSortAscending;
        private System.Windows.Forms.ToolStripMenuItem tsmiNodesSortDescending;
        private System.Windows.Forms.ToolStripMenuItem tsmiMessages;
        private System.Windows.Forms.ToolStripMenuItem tsmiMessagesLog;
        private System.Windows.Forms.ToolStripMenuItem tsmiMessagesClear;
        private System.Windows.Forms.ToolStripMenuItem tsmiMessagesCompose;
        private System.Windows.Forms.Splitter splitterNodes;
        private System.Windows.Forms.Splitter splitterMessages;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.ToolStripMenuItem tsmiComms;
        private System.Windows.Forms.ToolStripMenuItem tsmiCommsStart;
        private System.Windows.Forms.ToolStripMenuItem tsmiCommsStop;
        private System.Windows.Forms.ToolStripSeparator tssComms1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCommsConnection;
        private System.Windows.Forms.ToolStripStatusLabel tsslConnection;
        private System.Windows.Forms.ToolStripStatusLabel tsslConnectionInfo;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.PictureBox pictureBoxNodes;
    }
}
