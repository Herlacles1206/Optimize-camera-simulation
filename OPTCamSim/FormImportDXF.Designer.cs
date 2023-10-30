namespace OPTCAMSim
{
    partial class FormImportDXF
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
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.nudHScale = new System.Windows.Forms.NumericUpDown();
            this.bnFileOpen = new System.Windows.Forms.Button();
            this.bnStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbHor = new System.Windows.Forms.Label();
            this.lbVer = new System.Windows.Forms.Label();
            this.nudVScale = new System.Windows.Forms.NumericUpDown();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.nudBulgePrec = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.chkDoNotRescale = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudHScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBulgePrec)).BeginInit();
            this.SuspendLayout();
            // 
            // tbFileName
            // 
            this.tbFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFileName.Location = new System.Drawing.Point(20, 34);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(369, 20);
            this.tbFileName.TabIndex = 0;
            this.toolTip1.SetToolTip(this.tbFileName, "Provide relevant .DXF file name");
            // 
            // nudHScale
            // 
            this.nudHScale.DecimalPlaces = 3;
            this.nudHScale.Enabled = false;
            this.nudHScale.Location = new System.Drawing.Point(20, 83);
            this.nudHScale.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudHScale.Name = "nudHScale";
            this.nudHScale.Size = new System.Drawing.Size(95, 20);
            this.nudHScale.TabIndex = 1;
            this.toolTip1.SetToolTip(this.nudHScale, "Set tool maximum width, mm");
            this.nudHScale.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // bnFileOpen
            // 
            this.bnFileOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnFileOpen.Location = new System.Drawing.Point(395, 32);
            this.bnFileOpen.Name = "bnFileOpen";
            this.bnFileOpen.Size = new System.Drawing.Size(75, 23);
            this.bnFileOpen.TabIndex = 3;
            this.bnFileOpen.Text = "Browse...";
            this.bnFileOpen.UseVisualStyleBackColor = true;
            this.bnFileOpen.Click += new System.EventHandler(this.bnFileOpen_Click);
            // 
            // bnStart
            // 
            this.bnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnStart.Location = new System.Drawing.Point(397, 80);
            this.bnStart.Name = "bnStart";
            this.bnStart.Size = new System.Drawing.Size(75, 23);
            this.bnStart.TabIndex = 4;
            this.bnStart.Text = "Start";
            this.bnStart.UseVisualStyleBackColor = true;
            this.bnStart.Click += new System.EventHandler(this.bnStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select .DXF file with tool contour:";
            // 
            // lbHor
            // 
            this.lbHor.AutoSize = true;
            this.lbHor.Enabled = false;
            this.lbHor.Location = new System.Drawing.Point(17, 67);
            this.lbHor.Name = "lbHor";
            this.lbHor.Size = new System.Drawing.Size(107, 13);
            this.lbHor.TabIndex = 6;
            this.lbHor.Text = "Horizontal scale, mm:";
            // 
            // lbVer
            // 
            this.lbVer.AutoSize = true;
            this.lbVer.Enabled = false;
            this.lbVer.Location = new System.Drawing.Point(141, 67);
            this.lbVer.Name = "lbVer";
            this.lbVer.Size = new System.Drawing.Size(95, 13);
            this.lbVer.TabIndex = 7;
            this.lbVer.Text = "Vertical scale, mm:";
            // 
            // nudVScale
            // 
            this.nudVScale.DecimalPlaces = 3;
            this.nudVScale.Enabled = false;
            this.nudVScale.Location = new System.Drawing.Point(144, 83);
            this.nudVScale.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudVScale.Name = "nudVScale";
            this.nudVScale.Size = new System.Drawing.Size(95, 20);
            this.nudVScale.TabIndex = 8;
            this.toolTip1.SetToolTip(this.nudVScale, "Set tool total height, mm");
            this.nudVScale.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // nudBulgePrec
            // 
            this.nudBulgePrec.Location = new System.Drawing.Point(268, 83);
            this.nudBulgePrec.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudBulgePrec.Name = "nudBulgePrec";
            this.nudBulgePrec.Size = new System.Drawing.Size(95, 20);
            this.nudBulgePrec.TabIndex = 10;
            this.toolTip1.SetToolTip(this.nudBulgePrec, "Set tool bulge precision, points");
            this.nudBulgePrec.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(265, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Bulge precision, points:";
            // 
            // chkDoNotRescale
            // 
            this.chkDoNotRescale.AutoSize = true;
            this.chkDoNotRescale.Checked = true;
            this.chkDoNotRescale.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDoNotRescale.Location = new System.Drawing.Point(20, 109);
            this.chkDoNotRescale.Name = "chkDoNotRescale";
            this.chkDoNotRescale.Size = new System.Drawing.Size(95, 17);
            this.chkDoNotRescale.TabIndex = 11;
            this.chkDoNotRescale.Text = "Do not rescale";
            this.chkDoNotRescale.UseVisualStyleBackColor = true;
            this.chkDoNotRescale.CheckedChanged += new System.EventHandler(this.chkDoNotRescale_CheckedChanged);
            // 
            // FormImportDXF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 145);
            this.Controls.Add(this.chkDoNotRescale);
            this.Controls.Add(this.nudBulgePrec);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nudVScale);
            this.Controls.Add(this.lbVer);
            this.Controls.Add(this.lbHor);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bnStart);
            this.Controls.Add(this.bnFileOpen);
            this.Controls.Add(this.nudHScale);
            this.Controls.Add(this.tbFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimumSize = new System.Drawing.Size(500, 150);
            this.Name = "FormImportDXF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import data from DXF";
            ((System.ComponentModel.ISupportInitialize)(this.nudHScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBulgePrec)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.NumericUpDown nudHScale;
        private System.Windows.Forms.Button bnFileOpen;
        private System.Windows.Forms.Button bnStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbHor;
        private System.Windows.Forms.Label lbVer;
        private System.Windows.Forms.NumericUpDown nudVScale;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.NumericUpDown nudBulgePrec;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkDoNotRescale;
    }
}