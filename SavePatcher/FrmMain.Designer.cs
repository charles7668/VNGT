namespace SavePatcher
{
    partial class FrmMain
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
            cmbConfigList = new ComboBox();
            btnPatch = new Button();
            txtLog = new TextBox();
            SuspendLayout();
            // 
            // cmbConfigList
            // 
            cmbConfigList.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 136);
            cmbConfigList.FormattingEnabled = true;
            cmbConfigList.Location = new Point(30, 29);
            cmbConfigList.Name = "cmbConfigList";
            cmbConfigList.Size = new Size(563, 49);
            cmbConfigList.TabIndex = 0;
            // 
            // btnPatch
            // 
            btnPatch.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 136);
            btnPatch.Location = new Point(611, 29);
            btnPatch.Name = "btnPatch";
            btnPatch.Size = new Size(155, 48);
            btnPatch.TabIndex = 1;
            btnPatch.Text = "Patch";
            btnPatch.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(30, 104);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(736, 248);
            txtLog.TabIndex = 2;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(812, 387);
            Controls.Add(txtLog);
            Controls.Add(btnPatch);
            Controls.Add(cmbConfigList);
            Name = "FrmMain";
            Text = "Save Patcher";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbConfigList;
        private Button btnPatch;
        private TextBox txtLog;
    }
}
