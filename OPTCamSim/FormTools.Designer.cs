namespace OPTCAMSim
{
    partial class FormTools
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTools));
            this.pnlToolView = new System.Windows.Forms.Panel();
            this.glControl1 = new OPTCAMSim.GlControlExt();
            this.pnlToolParams = new System.Windows.Forms.Panel();
            this.dvParams = new System.Windows.Forms.DataGridView();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblParams = new System.Windows.Forms.Label();
            this.cmbToolType = new System.Windows.Forms.ComboBox();
            this.lbToolName = new System.Windows.Forms.Label();
            this.tbToolName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.bnImportDXF = new System.Windows.Forms.Button();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.pnlToolList = new System.Windows.Forms.Panel();
            this.LbTools = new System.Windows.Forms.ListBox();
            this.flpButtonsToolList = new System.Windows.Forms.FlowLayoutPanel();
            this.bnAddNewTool = new System.Windows.Forms.Button();
            this.bnEditTool = new System.Windows.Forms.Button();
            this.bnCopy = new System.Windows.Forms.Button();
            this.bnDeleteTool = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pnlToolView.SuspendLayout();
            this.pnlToolParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dvParams)).BeginInit();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlToolList.SuspendLayout();
            this.flpButtonsToolList.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlToolView
            // 
            this.pnlToolView.Controls.Add(this.glControl1);
            this.pnlToolView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlToolView.Location = new System.Drawing.Point(185, 0);
            this.pnlToolView.Name = "pnlToolView";
            this.pnlToolView.Size = new System.Drawing.Size(301, 423);
            this.pnlToolView.TabIndex = 1;
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(301, 423);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);            
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // pnlToolParams
            // 
            this.pnlToolParams.Controls.Add(this.dvParams);
            this.pnlToolParams.Controls.Add(this.panel1);
            this.pnlToolParams.Controls.Add(this.flowLayoutPanel1);
            this.pnlToolParams.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlToolParams.Enabled = false;
            this.pnlToolParams.Location = new System.Drawing.Point(486, 0);
            this.pnlToolParams.Name = "pnlToolParams";
            this.pnlToolParams.Size = new System.Drawing.Size(217, 423);
            this.pnlToolParams.TabIndex = 2;
            // 
            // dvParams
            // 
            this.dvParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvParams.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Property,
            this.Value});
            this.dvParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dvParams.Location = new System.Drawing.Point(0, 87);
            this.dvParams.Name = "dvParams";
            this.dvParams.RowHeadersVisible = false;
            this.dvParams.Size = new System.Drawing.Size(217, 293);
            this.dvParams.TabIndex = 3;
            // 
            // Property
            // 
            this.Property.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Property.HeaderText = "Property";
            this.Property.Name = "Property";
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblParams);
            this.panel1.Controls.Add(this.cmbToolType);
            this.panel1.Controls.Add(this.lbToolName);
            this.panel1.Controls.Add(this.tbToolName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(217, 87);
            this.panel1.TabIndex = 4;
            // 
            // lblParams
            // 
            this.lblParams.AutoSize = true;
            this.lblParams.Location = new System.Drawing.Point(7, 71);
            this.lblParams.Name = "lblParams";
            this.lblParams.Size = new System.Drawing.Size(0, 13);
            this.lblParams.TabIndex = 3;
            // 
            // cmbToolType
            // 
            this.cmbToolType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbToolType.Items.AddRange(new object[] {
            "Flat",
            "Drill",
            "Ball",
            "Bull",
            "VBit1",
            "VBit2",
            "VBit3",
            "VBit4",
            "VBit5",
            "VBit6",
            "VBit7",
            "VBit8",
            "VBit9",
            "Custom"});
            this.cmbToolType.Location = new System.Drawing.Point(68, 37);
            this.cmbToolType.Name = "cmbToolType";
            this.cmbToolType.Size = new System.Drawing.Size(133, 21);
            this.cmbToolType.TabIndex = 0;
            this.cmbToolType.SelectedIndexChanged += new System.EventHandler(this.cmbToolType_SelectedIndexChanged);
            // 
            // lbToolName
            // 
            this.lbToolName.AutoSize = true;
            this.lbToolName.Location = new System.Drawing.Point(2, 14);
            this.lbToolName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.lbToolName.Name = "lbToolName";
            this.lbToolName.Size = new System.Drawing.Size(60, 13);
            this.lbToolName.TabIndex = 0;
            this.lbToolName.Text = "Tool name:";
            // 
            // tbToolName
            // 
            this.tbToolName.Location = new System.Drawing.Point(68, 11);
            this.tbToolName.Name = "tbToolName";
            this.tbToolName.Size = new System.Drawing.Size(133, 20);
            this.tbToolName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 41);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tool type:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.bnImportDXF);
            this.flowLayoutPanel1.Controls.Add(this.bnSave);
            this.flowLayoutPanel1.Controls.Add(this.bnCancel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 380);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(217, 43);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // bnImportDXF
            // 
            this.bnImportDXF.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bnImportDXF.BackgroundImage")));
            this.bnImportDXF.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.bnImportDXF.Location = new System.Drawing.Point(3, 3);
            this.bnImportDXF.Name = "bnImportDXF";
            this.bnImportDXF.Size = new System.Drawing.Size(39, 35);
            this.bnImportDXF.TabIndex = 3;
            this.toolTip1.SetToolTip(this.bnImportDXF, "Import tool shape from .dxf");
            this.bnImportDXF.UseVisualStyleBackColor = true;
            this.bnImportDXF.Click += new System.EventHandler(this.bnImportDXF_Click);
            // 
            // bnSave
            // 
            this.bnSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bnSave.BackgroundImage")));
            this.bnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bnSave.Location = new System.Drawing.Point(48, 3);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(39, 35);
            this.bnSave.TabIndex = 2;
            this.toolTip1.SetToolTip(this.bnSave, "Save changes");
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bnCancel.BackgroundImage")));
            this.bnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bnCancel.Location = new System.Drawing.Point(93, 3);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(39, 35);
            this.bnCancel.TabIndex = 1;
            this.toolTip1.SetToolTip(this.bnCancel, "Cancel changes");
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // pnlToolList
            // 
            this.pnlToolList.Controls.Add(this.LbTools);
            this.pnlToolList.Controls.Add(this.flpButtonsToolList);
            this.pnlToolList.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlToolList.Location = new System.Drawing.Point(0, 0);
            this.pnlToolList.Name = "pnlToolList";
            this.pnlToolList.Size = new System.Drawing.Size(185, 423);
            this.pnlToolList.TabIndex = 3;
            // 
            // LbTools
            // 
            this.LbTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LbTools.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LbTools.FormattingEnabled = true;
            this.LbTools.ItemHeight = 16;
            this.LbTools.Location = new System.Drawing.Point(0, 0);
            this.LbTools.Name = "LbTools";
            this.LbTools.Size = new System.Drawing.Size(185, 380);
            this.LbTools.TabIndex = 2;
            this.LbTools.SelectedIndexChanged += new System.EventHandler(this.LbTools_SelectedIndexChanged);
            // 
            // flpButtonsToolList
            // 
            this.flpButtonsToolList.Controls.Add(this.bnAddNewTool);
            this.flpButtonsToolList.Controls.Add(this.bnEditTool);
            this.flpButtonsToolList.Controls.Add(this.bnCopy);
            this.flpButtonsToolList.Controls.Add(this.bnDeleteTool);
            this.flpButtonsToolList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpButtonsToolList.Location = new System.Drawing.Point(0, 380);
            this.flpButtonsToolList.Name = "flpButtonsToolList";
            this.flpButtonsToolList.Size = new System.Drawing.Size(185, 43);
            this.flpButtonsToolList.TabIndex = 1;
            // 
            // bnAddNewTool
            // 
            this.bnAddNewTool.BackColor = System.Drawing.SystemColors.Control;
            this.bnAddNewTool.Image = ((System.Drawing.Image)(resources.GetObject("bnAddNewTool.Image")));
            this.bnAddNewTool.Location = new System.Drawing.Point(3, 3);
            this.bnAddNewTool.Name = "bnAddNewTool";
            this.bnAddNewTool.Size = new System.Drawing.Size(39, 35);
            this.bnAddNewTool.TabIndex = 0;
            this.toolTip1.SetToolTip(this.bnAddNewTool, "Add new tool");
            this.bnAddNewTool.UseVisualStyleBackColor = false;
            this.bnAddNewTool.Click += new System.EventHandler(this.bnAddNewTool_Click);
            // 
            // bnEditTool
            // 
            this.bnEditTool.Image = ((System.Drawing.Image)(resources.GetObject("bnEditTool.Image")));
            this.bnEditTool.Location = new System.Drawing.Point(48, 3);
            this.bnEditTool.Name = "bnEditTool";
            this.bnEditTool.Size = new System.Drawing.Size(39, 35);
            this.bnEditTool.TabIndex = 2;
            this.toolTip1.SetToolTip(this.bnEditTool, "Edit selected tool");
            this.bnEditTool.UseVisualStyleBackColor = true;
            this.bnEditTool.Click += new System.EventHandler(this.bnEditTool_Click);
            // 
            // bnCopy
            // 
            this.bnCopy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bnCopy.BackgroundImage")));
            this.bnCopy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bnCopy.Location = new System.Drawing.Point(93, 3);
            this.bnCopy.Name = "bnCopy";
            this.bnCopy.Size = new System.Drawing.Size(39, 35);
            this.bnCopy.TabIndex = 3;
            this.toolTip1.SetToolTip(this.bnCopy, "Copy selected tool");
            this.bnCopy.UseVisualStyleBackColor = true;
            this.bnCopy.Click += new System.EventHandler(this.bnCopy_Click);
            // 
            // bnDeleteTool
            // 
            this.bnDeleteTool.Image = ((System.Drawing.Image)(resources.GetObject("bnDeleteTool.Image")));
            this.bnDeleteTool.Location = new System.Drawing.Point(138, 3);
            this.bnDeleteTool.Name = "bnDeleteTool";
            this.bnDeleteTool.Size = new System.Drawing.Size(39, 35);
            this.bnDeleteTool.TabIndex = 1;
            this.toolTip1.SetToolTip(this.bnDeleteTool, "Remove selected tool");
            this.bnDeleteTool.UseVisualStyleBackColor = true;
            this.bnDeleteTool.Click += new System.EventHandler(this.bnDeleteTool_Click);
            // 
            // FormTools
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(703, 423);
            this.Controls.Add(this.pnlToolView);
            this.Controls.Add(this.pnlToolList);
            this.Controls.Add(this.pnlToolParams);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "FormTools";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tools gallery";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTools_FormClosing);
            this.Load += new System.EventHandler(this.FormTools_Load);
            this.Shown += new System.EventHandler(this.FormTools_Shown);
            this.pnlToolView.ResumeLayout(false);
            this.pnlToolParams.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dvParams)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.pnlToolList.ResumeLayout(false);
            this.flpButtonsToolList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlToolView;
        private System.Windows.Forms.Panel pnlToolParams;
        private System.Windows.Forms.Panel pnlToolList;
        private System.Windows.Forms.FlowLayoutPanel flpButtonsToolList;
        private System.Windows.Forms.Button bnAddNewTool;
        private System.Windows.Forms.Button bnDeleteTool;
        private System.Windows.Forms.Button bnEditTool;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ListBox LbTools;
        private OPTCAMSim.GlControlExt glControl1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
        private System.Windows.Forms.Label lbToolName;
        private System.Windows.Forms.TextBox tbToolName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbToolType;
        private System.Windows.Forms.DataGridView dvParams;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button bnCopy;
        private System.Windows.Forms.Label lblParams;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.Button bnImportDXF;
    }
}