namespace DemoForm
{
	partial class MainForm
	{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.CentralPanel = new System.Windows.Forms.Panel();
            this.glControl1 = new OPTCAMSim.GlControlExt();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblPrc = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.gbTool = new System.Windows.Forms.GroupBox();
            this.pnlNestedPanels = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbPanels = new System.Windows.Forms.ComboBox();
            this.chkShowPartData = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.nudSpeed = new System.Windows.Forms.NumericUpDown();
            this.tbToolSpeed = new System.Windows.Forms.TrackBar();
            this.bnTopView = new System.Windows.Forms.Button();
            this.bnStop = new System.Windows.Forms.Button();
            this.bnPause = new System.Windows.Forms.Button();
            this.bnPlayM6 = new System.Windows.Forms.Button();
            this.bnStart = new System.Windows.Forms.Button();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.splitCodeOGC = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dvCode = new System.Windows.Forms.DataGridView();
            this.Line = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlsPanel = new System.Windows.Forms.Panel();
            this.gbNavigation = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.bnStep = new System.Windows.Forms.Button();
            this.gbPanel = new System.Windows.Forms.GroupBox();
            this.nudHeight = new System.Windows.Forms.NumericUpDown();
            this.nudLength = new System.Windows.Forms.NumericUpDown();
            this.nudWidth = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.CentralPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbTool.SuspendLayout();
            this.pnlNestedPanels.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbToolSpeed)).BeginInit();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCodeOGC)).BeginInit();
            this.splitCodeOGC.Panel1.SuspendLayout();
            this.splitCodeOGC.Panel2.SuspendLayout();
            this.splitCodeOGC.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dvCode)).BeginInit();
            this.ControlsPanel.SuspendLayout();
            this.gbNavigation.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.gbPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CentralPanel
            // 
            this.CentralPanel.Controls.Add(this.glControl1);
            this.CentralPanel.Controls.Add(this.panel1);
            this.CentralPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CentralPanel.Location = new System.Drawing.Point(0, 0);
            this.CentralPanel.Name = "CentralPanel";
            this.CentralPanel.Size = new System.Drawing.Size(429, 537);
            this.CentralPanel.TabIndex = 2;
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(-1, 0);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(430, 513);
            this.glControl1.TabIndex = 2;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblPrc);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 513);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(429, 24);
            this.panel1.TabIndex = 1;
            // 
            // lblPrc
            // 
            this.lblPrc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPrc.AutoSize = true;
            this.lblPrc.Location = new System.Drawing.Point(389, 3);
            this.lblPrc.Name = "lblPrc";
            this.lblPrc.Size = new System.Drawing.Size(0, 13);
            this.lblPrc.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(251, -6);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(132, 33);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 0;
            // 
            // gbTool
            // 
            this.gbTool.Controls.Add(this.pnlNestedPanels);
            this.gbTool.Controls.Add(this.panel3);
            this.gbTool.Controls.Add(this.bnTopView);
            this.gbTool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTool.Location = new System.Drawing.Point(0, 312);
            this.gbTool.Name = "gbTool";
            this.gbTool.Size = new System.Drawing.Size(176, 225);
            this.gbTool.TabIndex = 2;
            this.gbTool.TabStop = false;
            this.gbTool.Text = "Tool";
            // 
            // pnlNestedPanels
            // 
            this.pnlNestedPanels.Controls.Add(this.label5);
            this.pnlNestedPanels.Controls.Add(this.cmbPanels);
            this.pnlNestedPanels.Controls.Add(this.chkShowPartData);
            this.pnlNestedPanels.Location = new System.Drawing.Point(3, 88);
            this.pnlNestedPanels.Name = "pnlNestedPanels";
            this.pnlNestedPanels.Size = new System.Drawing.Size(167, 56);
            this.pnlNestedPanels.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Panel:";
            // 
            // cmbPanels
            // 
            this.cmbPanels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPanels.FormattingEnabled = true;
            this.cmbPanels.Location = new System.Drawing.Point(44, 4);
            this.cmbPanels.Name = "cmbPanels";
            this.cmbPanels.Size = new System.Drawing.Size(121, 21);
            this.cmbPanels.TabIndex = 28;
            this.cmbPanels.SelectedIndexChanged += new System.EventHandler(this.cmbPanels_SelectedIndexChanged);
            // 
            // chkShowPartData
            // 
            this.chkShowPartData.AutoSize = true;
            this.chkShowPartData.Location = new System.Drawing.Point(44, 31);
            this.chkShowPartData.Name = "chkShowPartData";
            this.chkShowPartData.Size = new System.Drawing.Size(98, 17);
            this.chkShowPartData.TabIndex = 27;
            this.chkShowPartData.Text = "Show part data";
            this.chkShowPartData.UseVisualStyleBackColor = true;
            this.chkShowPartData.CheckedChanged += new System.EventHandler(this.chkShowPartData_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.nudSpeed);
            this.panel3.Controls.Add(this.tbToolSpeed);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 16);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(170, 57);
            this.panel3.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Tool speed:";
            // 
            // nudSpeed
            // 
            this.nudSpeed.Location = new System.Drawing.Point(94, 5);
            this.nudSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSpeed.Name = "nudSpeed";
            this.nudSpeed.Size = new System.Drawing.Size(70, 20);
            this.nudSpeed.TabIndex = 25;
            this.nudSpeed.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudSpeed.ValueChanged += new System.EventHandler(this.nudSpeed_ValueChanged);
            // 
            // tbToolSpeed
            // 
            this.tbToolSpeed.Location = new System.Drawing.Point(6, 24);
            this.tbToolSpeed.Margin = new System.Windows.Forms.Padding(0);
            this.tbToolSpeed.Maximum = 100;
            this.tbToolSpeed.Minimum = 1;
            this.tbToolSpeed.Name = "tbToolSpeed";
            this.tbToolSpeed.Size = new System.Drawing.Size(158, 45);
            this.tbToolSpeed.TabIndex = 23;
            this.tbToolSpeed.TickFrequency = 10;
            this.tbToolSpeed.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.tbToolSpeed.Value = 50;
            this.tbToolSpeed.Scroll += new System.EventHandler(this.tbToolSpeed_Scroll);
            // 
            // bnTopView
            // 
            this.bnTopView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bnTopView.Location = new System.Drawing.Point(6, 150);
            this.bnTopView.Name = "bnTopView";
            this.bnTopView.Size = new System.Drawing.Size(164, 32);
            this.bnTopView.TabIndex = 24;
            this.bnTopView.Text = "Top View";
            this.toolTip1.SetToolTip(this.bnTopView, "Top View");
            this.bnTopView.UseVisualStyleBackColor = true;
            this.bnTopView.Click += new System.EventHandler(this.bnTopView_Click);
            // 
            // bnStop
            // 
            this.bnStop.Enabled = false;
            this.bnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bnStop.Location = new System.Drawing.Point(3, 155);
            this.bnStop.Name = "bnStop";
            this.bnStop.Size = new System.Drawing.Size(164, 32);
            this.bnStop.TabIndex = 19;
            this.bnStop.Text = "Stop";
            this.toolTip1.SetToolTip(this.bnStop, "Stop simulation and return to initial state");
            this.bnStop.UseVisualStyleBackColor = true;
            this.bnStop.Click += new System.EventHandler(this.bnStop_Click);
            // 
            // bnPause
            // 
            this.bnPause.Enabled = false;
            this.bnPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bnPause.Location = new System.Drawing.Point(3, 117);
            this.bnPause.Name = "bnPause";
            this.bnPause.Size = new System.Drawing.Size(164, 32);
            this.bnPause.TabIndex = 21;
            this.bnPause.Text = "Pause";
            this.toolTip1.SetToolTip(this.bnPause, "Pause simulation process");
            this.bnPause.UseVisualStyleBackColor = true;
            this.bnPause.Click += new System.EventHandler(this.bnPause_Click);
            // 
            // bnPlayM6
            // 
            this.bnPlayM6.AutoSize = true;
            this.bnPlayM6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bnPlayM6.Location = new System.Drawing.Point(3, 79);
            this.bnPlayM6.Name = "bnPlayM6";
            this.bnPlayM6.Size = new System.Drawing.Size(164, 32);
            this.bnPlayM6.TabIndex = 22;
            this.bnPlayM6.Text = "PlayM6";
            this.toolTip1.SetToolTip(this.bnPlayM6, "Run simulation until next tool change");
            this.bnPlayM6.UseVisualStyleBackColor = true;
            this.bnPlayM6.Click += new System.EventHandler(this.bnPlayM6_Click);
            // 
            // bnStart
            // 
            this.bnStart.AutoSize = true;
            this.bnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bnStart.Location = new System.Drawing.Point(3, 3);
            this.bnStart.Name = "bnStart";
            this.bnStart.Size = new System.Drawing.Size(164, 32);
            this.bnStart.TabIndex = 20;
            this.bnStart.Text = "Start";
            this.toolTip1.SetToolTip(this.bnStart, "Start simulation process");
            this.bnStart.UseVisualStyleBackColor = true;
            this.bnStart.Click += new System.EventHandler(this.bnStart_Click);
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.splitCodeOGC);
            this.MainPanel.Controls.Add(this.ControlsPanel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 24);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(784, 537);
            this.MainPanel.TabIndex = 4;
            // 
            // splitCodeOGC
            // 
            this.splitCodeOGC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCodeOGC.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitCodeOGC.Location = new System.Drawing.Point(176, 0);
            this.splitCodeOGC.Name = "splitCodeOGC";
            // 
            // splitCodeOGC.Panel1
            // 
            this.splitCodeOGC.Panel1.Controls.Add(this.groupBox1);
            this.splitCodeOGC.Panel1MinSize = 160;
            // 
            // splitCodeOGC.Panel2
            // 
            this.splitCodeOGC.Panel2.Controls.Add(this.CentralPanel);
            this.splitCodeOGC.Panel2MinSize = 400;
            this.splitCodeOGC.Size = new System.Drawing.Size(608, 537);
            this.splitCodeOGC.SplitterDistance = 175;
            this.splitCodeOGC.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dvCode);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 537);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Simulation code";
            // 
            // dvCode
            // 
            this.dvCode.AllowUserToAddRows = false;
            this.dvCode.AllowUserToDeleteRows = false;
            this.dvCode.AllowUserToResizeRows = false;
            this.dvCode.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dvCode.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvCode.ColumnHeadersVisible = false;
            this.dvCode.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Line,
            this.Code});
            this.dvCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dvCode.Location = new System.Drawing.Point(3, 16);
            this.dvCode.MultiSelect = false;
            this.dvCode.Name = "dvCode";
            this.dvCode.RowHeadersVisible = false;
            this.dvCode.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dvCode.Size = new System.Drawing.Size(169, 518);
            this.dvCode.TabIndex = 1;
            // 
            // Line
            // 
            this.Line.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Line.DataPropertyName = "Line";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Line.DefaultCellStyle = dataGridViewCellStyle3;
            this.Line.HeaderText = "Line";
            this.Line.Name = "Line";
            this.Line.ReadOnly = true;
            this.Line.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Line.Width = 5;
            // 
            // Code
            // 
            this.Code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Code.DataPropertyName = "Code";
            this.Code.HeaderText = "Code";
            this.Code.Name = "Code";
            this.Code.ReadOnly = true;
            this.Code.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Controls.Add(this.gbTool);
            this.ControlsPanel.Controls.Add(this.gbNavigation);
            this.ControlsPanel.Controls.Add(this.gbPanel);
            this.ControlsPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ControlsPanel.Location = new System.Drawing.Point(0, 0);
            this.ControlsPanel.Name = "ControlsPanel";
            this.ControlsPanel.Size = new System.Drawing.Size(176, 537);
            this.ControlsPanel.TabIndex = 3;
            // 
            // gbNavigation
            // 
            this.gbNavigation.Controls.Add(this.flowLayoutPanel1);
            this.gbNavigation.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbNavigation.Location = new System.Drawing.Point(0, 94);
            this.gbNavigation.Name = "gbNavigation";
            this.gbNavigation.Size = new System.Drawing.Size(176, 218);
            this.gbNavigation.TabIndex = 23;
            this.gbNavigation.TabStop = false;
            this.gbNavigation.Text = "Navigation";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.bnStart);
            this.flowLayoutPanel1.Controls.Add(this.bnStep);
            this.flowLayoutPanel1.Controls.Add(this.bnPlayM6);
            this.flowLayoutPanel1.Controls.Add(this.bnPause);
            this.flowLayoutPanel1.Controls.Add(this.bnStop);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(170, 199);
            this.flowLayoutPanel1.TabIndex = 23;
            // 
            // bnStep
            // 
            this.bnStep.AutoSize = true;
            this.bnStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bnStep.Location = new System.Drawing.Point(3, 41);
            this.bnStep.Name = "bnStep";
            this.bnStep.Size = new System.Drawing.Size(164, 32);
            this.bnStep.TabIndex = 23;
            this.bnStep.Text = "Step";
            this.toolTip1.SetToolTip(this.bnStep, "Make single step of simulation");
            this.bnStep.UseVisualStyleBackColor = true;
            this.bnStep.Click += new System.EventHandler(this.bnStep_Click);
            // 
            // gbPanel
            // 
            this.gbPanel.Controls.Add(this.nudHeight);
            this.gbPanel.Controls.Add(this.nudLength);
            this.gbPanel.Controls.Add(this.nudWidth);
            this.gbPanel.Controls.Add(this.label3);
            this.gbPanel.Controls.Add(this.label2);
            this.gbPanel.Controls.Add(this.label1);
            this.gbPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbPanel.Location = new System.Drawing.Point(0, 0);
            this.gbPanel.Name = "gbPanel";
            this.gbPanel.Size = new System.Drawing.Size(176, 94);
            this.gbPanel.TabIndex = 6;
            this.gbPanel.TabStop = false;
            this.gbPanel.Text = "Panel";
            // 
            // nudHeight
            // 
            this.nudHeight.DecimalPlaces = 2;
            this.nudHeight.Location = new System.Drawing.Point(67, 68);
            this.nudHeight.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudHeight.Name = "nudHeight";
            this.nudHeight.Size = new System.Drawing.Size(103, 20);
            this.nudHeight.TabIndex = 8;
            this.toolTip1.SetToolTip(this.nudHeight, "Z-axis panel size in mm");
            this.nudHeight.ValueChanged += new System.EventHandler(this.nudHeight_ValueChanged);
            // 
            // nudLength
            // 
            this.nudLength.DecimalPlaces = 2;
            this.nudLength.Location = new System.Drawing.Point(67, 42);
            this.nudLength.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudLength.Name = "nudLength";
            this.nudLength.Size = new System.Drawing.Size(103, 20);
            this.nudLength.TabIndex = 7;
            this.toolTip1.SetToolTip(this.nudLength, "Y-axis panel size in mm");
            this.nudLength.ValueChanged += new System.EventHandler(this.nudLength_ValueChanged);
            // 
            // nudWidth
            // 
            this.nudWidth.DecimalPlaces = 2;
            this.nudWidth.Location = new System.Drawing.Point(67, 16);
            this.nudWidth.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudWidth.Name = "nudWidth";
            this.nudWidth.Size = new System.Drawing.Size(103, 20);
            this.nudWidth.TabIndex = 6;
            this.toolTip1.SetToolTip(this.nudWidth, "X-axis panel size in mm");
            this.nudWidth.ValueChanged += new System.EventHandler(this.nudWidth_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Height";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Length";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Width";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.settingsToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LoadProgramToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // LoadProgramToolStripMenuItem
            // 
            this.LoadProgramToolStripMenuItem.Name = "LoadProgramToolStripMenuItem";
            this.LoadProgramToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.LoadProgramToolStripMenuItem.Text = "Load toolpath from file...";
            this.LoadProgramToolStripMenuItem.ToolTipText = "Loads toolpath from file";
            this.LoadProgramToolStripMenuItem.Click += new System.EventHandler(this.LoadProgramToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.ToolTipText = "Closes program";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            this.toolsToolStripMenuItem.Click += new System.EventHandler(this.toolsToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem1.Text = "Settings";
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.settingsToolStripMenuItem1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.Text = "Simulate";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.Shown += new System.EventHandler(this.DemoForm_Shown);
            this.CentralPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gbTool.ResumeLayout(false);
            this.pnlNestedPanels.ResumeLayout(false);
            this.pnlNestedPanels.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbToolSpeed)).EndInit();
            this.MainPanel.ResumeLayout(false);
            this.splitCodeOGC.Panel1.ResumeLayout(false);
            this.splitCodeOGC.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCodeOGC)).EndInit();
            this.splitCodeOGC.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dvCode)).EndInit();
            this.ControlsPanel.ResumeLayout(false);
            this.gbNavigation.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.gbPanel.ResumeLayout(false);
            this.gbPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel CentralPanel;
        private System.Windows.Forms.GroupBox gbTool;
        private System.Windows.Forms.Button bnStart;
        private System.Windows.Forms.Button bnStop;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem LoadProgramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblPrc;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudSpeed;
        private System.Windows.Forms.TrackBar tbToolSpeed;
        private System.Windows.Forms.Button bnPause;
        private System.Windows.Forms.Panel ControlsPanel;
        private System.Windows.Forms.Button bnPlayM6;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.GroupBox gbNavigation;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox gbPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bnStep;
        private System.Windows.Forms.SplitContainer splitCodeOGC;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dvCode;
        private System.Windows.Forms.NumericUpDown nudHeight;
        private System.Windows.Forms.NumericUpDown nudLength;
        private System.Windows.Forms.NumericUpDown nudWidth;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.CheckBox chkShowPartData;
        private System.Windows.Forms.Panel pnlNestedPanels;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbPanels;
        private System.Windows.Forms.DataGridViewTextBoxColumn Line;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.Button bnTopView;
        private OPTCAMSim.GlControlExt glControl1;
    }
}

