using System.Diagnostics;

namespace VNGT
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnOpenSavePatcher_Click(object sender, EventArgs e)
        {
            string path = "SavePatcher.exe";
            Process.Start(path);
        }
    }
}