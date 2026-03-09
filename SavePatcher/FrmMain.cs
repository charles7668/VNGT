using Microsoft.Extensions.DependencyInjection;
using SavePatcher.Configs;
using SavePatcher.Logs;
using SavePatcher.Models;
using System.Diagnostics;

namespace SavePatcher
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            _configReader = Program.ServiceProvider.GetRequiredService<IConfigReader<SavePatcherConfig[]>>();
        }

        private const string CHANGE_LINE = "\r\n";

        // config cache
        private readonly Dictionary<string, SavePatcherConfig> _configCache = new();

        // config reader
        private readonly IConfigReader<SavePatcherConfig[]> _configReader;

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAllConfigFileName();
        }

        private void LoadAllConfigFileName()
        {
            Directory.CreateDirectory(Program.ConfigPath);
            string[] files = Directory.GetFiles(Program.ConfigPath, "*.yaml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                IConfigReader<SavePatcherConfig> tempReader =
                    Program.ServiceProvider.GetRequiredService<IConfigReader<SavePatcherConfig>>();
                Result<SavePatcherConfig> tempConfigResult = tempReader.Read(content);
                Result<SavePatcherConfig[]> configResult;
                if (!tempConfigResult.Success)
                {
                    configResult = _configReader.Read(content);
                }
                else
                {
                    if (tempConfigResult.Value == null)
                    {
                        continue;
                    }

                    configResult = Result<SavePatcherConfig[]>.Ok([tempConfigResult.Value]);
                }

                if (!configResult.Success
                    || configResult.Value == null
                    || configResult.Value.Length == 0)
                {
                    continue;
                }

                foreach (SavePatcherConfig config in configResult.Value)
                {
                    string configName = config.ConfigName;
                    if (_configCache.ContainsKey(configName) || string.IsNullOrEmpty(configName.Trim()))
                    {
                        continue;
                    }

                    cmbConfigList.Items.Add(configName);
                    _configCache[configName] = config;
                }
            }
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            ClearLogText();
            string configName = cmbConfigList.Text;
            if (!_configCache.TryGetValue(configName, out SavePatcherConfig? config))
            {
                ErrorMessage("config not found");
                return;
            }

            InfoMessage("start patch...");

            Patcher.SavePatcher savePatcher = Program.ServiceProvider.GetRequiredService<Patcher.SavePatcher>();
            savePatcher.FilePath = config.FilePath;
            savePatcher.DestinationPath = config.DestinationPath;
            savePatcher.PatchFiles = config.PatchFiles;
            savePatcher.ZipPassword = config.ZipPassword;
            savePatcher.LogCallbacks = new LogCallbacks();
            savePatcher.LogCallbacks.LogInfoEvent += (_, args) => InfoMessage(args.Message);
            savePatcher.LogCallbacks.LogErrorEvent += (_, args) => ErrorMessage(args.Message);
            await savePatcher.PatchAsync();

            InfoMessage("end patch");
        }

        /// <summary>
        /// Add info message to log text
        /// </summary>
        /// <param name="message"></param>
        private void InfoMessage(string message)
        {
            txtLog.Text += @"[Info] " + message + CHANGE_LINE;
        }

        /// <summary>
        /// Add error message to log text
        /// </summary>
        /// <param name="message"></param>
        private void ErrorMessage(string message)
        {
            txtLog.Text += @"[Error] " + message + CHANGE_LINE;
        }

        /// <summary>
        /// clear log text
        /// </summary>
        private void ClearLogText()
        {
            txtLog.Text = string.Empty;
        }

        private void btnOpenConfigFolder_Click(object sender, EventArgs e)
        {
            // open config folder
            Process.Start("explorer.exe", Program.ConfigPath);
        }
    }
}