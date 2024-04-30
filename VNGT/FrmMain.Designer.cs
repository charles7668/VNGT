namespace VNGT
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
            btnOpenSavePatcher = new Button();
            SuspendLayout();
            // 
            // btnOpenSavePatcher
            // 
            btnOpenSavePatcher.Font = new Font("Microsoft JhengHei UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 136);
            btnOpenSavePatcher.Location = new Point(29, 24);
            btnOpenSavePatcher.Name = "btnOpenSavePatcher";
            btnOpenSavePatcher.Size = new Size(213, 65);
            btnOpenSavePatcher.TabIndex = 0;
            btnOpenSavePatcher.Text = "Save Patcher";
            btnOpenSavePatcher.UseVisualStyleBackColor = true;
            btnOpenSavePatcher.Click += btnOpenSavePatcher_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(508, 136);
            Controls.Add(btnOpenSavePatcher);
            Name = "FrmMain";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button btnOpenSavePatcher;
    }
}
