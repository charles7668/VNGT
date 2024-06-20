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

        // config name cache to avoid duplicate config name and find origin file path
        private readonly Dictionary<string, string> _configNameCache = new();

        // config reader
        private readonly IConfigReader<SavePatcherConfig[]> _configReader;

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAllConfigFileName();
        }

        /// <summary>
        /// load all config file name from <see cref="Program.ConfigPath" />
        /// </summary>
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

                string configName = configResult.Value![0].ConfigName;
                if (_configNameCache.ContainsKey(configName) || string.IsNullOrEmpty(configName.Trim()))
                {
                    continue;
                }

                cmbConfigList.Items.Add(configResult.Value[0].ConfigName);
                _configNameCache[configName] = file;
            }
        }

        private async void btnPatch_Click(object sender, EventArgs e)
        {
            ClearLogText();
            string configName = cmbConfigList.Text;
            if (!_configNameCache.TryGetValue(configName, out string? configFilePath)
                || !File.Exists(configFilePath))
            {
                ErrorMessage("config file not found");
                return;
            }

            InfoMessage("start load config file...");
            string content = await File.ReadAllTextAsync(configFilePath);
            Result<SavePatcherConfig[]> configResult = _configReader.Read(content);
            if (!configResult.Success)
            {
                ErrorMessage(configResult.Message);
                return;
            }

            InfoMessage("end log config file");
            SavePatcherConfig[] configs = configResult.Value!;
            foreach (SavePatcherConfig config in configs)
            {
                Patcher.SavePatcher savePatcher = Program.ServiceProvider.GetRequiredService<Patcher.SavePatcher>();
                savePatcher.FilePath = config.FilePath;
                savePatcher.DestinationPath = config.DestinationPath;
                savePatcher.PatchFiles = config.PatchFiles;
                savePatcher.ZipPassword = config.ZipPassword;
                savePatcher.LogCallbacks = new LogCallbacks();
                savePatcher.LogCallbacks.LogInfoEvent += (_, args) => InfoMessage(args.Message);
                savePatcher.LogCallbacks.LogErrorEvent += (_, args) => ErrorMessage(args.Message);
                await savePatcher.PatchAsync();
            }
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