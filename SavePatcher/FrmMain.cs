using Microsoft.Extensions.DependencyInjection;
using SavePatcher.Configs;
using SavePatcher.Models;

namespace SavePatcher
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            _configReader = Program.ServiceProvider.GetRequiredService<IConfigReader<SavePatcherConfig>>();
        }

        // config name cache to avoid duplicate config name and find origin file path
        private readonly Dictionary<string, string> _configNameCache = new();

        // config reader
        private readonly IConfigReader<SavePatcherConfig> _configReader;

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAllConfigFileName();
        }

        /// <summary>
        /// load all config file name from <see cref="Program.ConfigPath" />
        /// </summary>
        private void LoadAllConfigFileName()
        {
            string[] files = Directory.GetFiles(Program.ConfigPath, "*.yaml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                Result<SavePatcherConfig> configResult = _configReader.Read(File.ReadAllText(file));
                if (!configResult.Success || configResult.Value == null)
                {
                    continue;
                }

                string configName = configResult.Value.ConfigName;
                if (_configNameCache.ContainsKey(configName) || string.IsNullOrEmpty(configName.Trim()))
                {
                    continue;
                }

                cmbConfigList.Items.Add(configResult.Value.ConfigName);
                _configNameCache[configName] = file;
            }
        }
    }
}