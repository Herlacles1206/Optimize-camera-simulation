namespace OPTCAMSim
{
    partial class GlControlExt
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GlControlExt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "GlControlExt";
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.GlControlExt_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GlControlExt_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GlControlExt_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GlControlExt_MouseUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.GlControlExt_PreviewKeyDown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
