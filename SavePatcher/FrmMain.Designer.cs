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
            btnOpenConfigFolder = new Button();
            SuspendLayout();
            // 
            // cmbConfigList
            // 
            cmbConfigList.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 136);
            cmbConfigList.FormattingEnabled = true;
            cmbConfigList.Location = new Point(46, 118);
            cmbConfigList.Margin = new Padding(5, 5, 5, 5);
            cmbConfigList.Name = "cmbConfigList";
            cmbConfigList.Size = new Size(882, 69);
            cmbConfigList.TabIndex = 0;
            // 
            // btnPatch
            // 
            btnPatch.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 136);
            btnPatch.Location = new Point(959, 118);
            btnPatch.Margin = new Padding(5, 5, 5, 5);
            btnPatch.Name = "btnPatch";
            btnPatch.Size = new Size(244, 74);
            btnPatch.TabIndex = 1;
            btnPatch.Text = "Patch";
            btnPatch.UseVisualStyleBackColor = true;
            btnPatch.Click += btnPatch_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(46, 233);
            txtLog.Margin = new Padding(5, 5, 5, 5);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(1154, 378);
            txtLog.TabIndex = 2;
            // 
            // btnOpenConfigFolder
            // 
            btnOpenConfigFolder.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 136);
            btnOpenConfigFolder.Location = new Point(46, 25);
            btnOpenConfigFolder.Margin = new Padding(5);
            btnOpenConfigFolder.Name = "btnOpenConfigFolder";
            btnOpenConfigFolder.Size = new Size(579, 74);
            btnOpenConfigFolder.TabIndex = 1;
            btnOpenConfigFolder.Text = "Open Config Folder";
            btnOpenConfigFolder.UseVisualStyleBackColor = true;
            btnOpenConfigFolder.Click += btnOpenConfigFolder_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1315, 726);
            Controls.Add(txtLog);
            Controls.Add(btnOpenConfigFolder);
            Controls.Add(btnPatch);
            Controls.Add(cmbConfigList);
            Margin = new Padding(5, 5, 5, 5);
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
        private Button btnOpenConfigFolder;
    }
}
