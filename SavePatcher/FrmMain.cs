using Microsoft.Extensions.DependencyInjection;
using SavePatcher.Configs;
using SavePatcher.Extractor;
using SavePatcher.Models;

namespace SavePatcher
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            _configReader = Program.ServiceProvider.GetRequiredService<IConfigReader<SavePatcherConfig>>();
            _extractorFactory = Program.ServiceProvider.GetRequiredService<ExtractorFactory>();
        }

        private const string CHANGE_LINE = "\r\n";

        // config name cache to avoid duplicate config name and find origin file path
        private readonly Dictionary<string, string> _configNameCache = new();

        // config reader
        private readonly IConfigReader<SavePatcherConfig> _configReader;

        // extractor factory
        private readonly ExtractorFactory _extractorFactory;

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
            Result<SavePatcherConfig> configResult = _configReader.Read(content);
            if (!configResult.Success)
            {
                ErrorMessage(configResult.Message);
                return;
            }

            InfoMessage("end log config file");
            var config = configResult.Value!;

            InfoMessage($"start check save file {config.FilePath} exist...");
            if (File.Exists(config.FilePath))
            {
                ErrorMessage("file not exist");
                return;
            }

            InfoMessage("file check success");
            InfoMessage("start extract file...");
            string extension = Path.GetExtension(config.FilePath);
            var extractor = _extractorFactory.GetExtractor(extension);
            if (extractor == null)
            {
                ErrorMessage($"extractor for extension : '{extension}' not found");
                return;
            }

            Result<string> extractResult = await extractor.ExtractAsync(config.FilePath,
                new ExtractOption { Password = config.ZipPassword, SpecificFiles = config.PatchFiles });

            if (!extractResult.Success || extractResult.Value == null)
            {
                ErrorMessage(extractResult.Message);
                return;
            }

            if (config.PatchFiles.Length == 0)
            {
                InfoMessage("extract all file success");
            }
            else
            {
                foreach (string configPatchFile in config.PatchFiles)
                {
                    InfoMessage($"extract {configPatchFile} success");
                }
            }

            string destinationPath = config.DestinationPath;
            if (string.IsNullOrEmpty(destinationPath))
            {
                var folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    destinationPath = folderBrowserDialog.SelectedPath;
                }
                else
                {
                    ErrorMessage("patch canceled");
                    return;
                }
            }

            if (Directory.Exists(destinationPath))
            {
                ErrorMessage("destination path not exist");
                return;
            }

            InfoMessage($"start copy to {destinationPath}");
            try
            {
                if (config.PatchFiles.Length == 0)
                {
                    CopyDirectory(extractResult.Value, destinationPath);
                }
                else
                {
                    foreach (string configPatchFile in config.PatchFiles)
                    {
                        string source = Path.Combine(extractResult.Value, configPatchFile);
                        File.Copy(source, destinationPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                return;
            }

            InfoMessage("patch complete");
        }

        /// <summary>
        /// copy all file in source directory to destination directory
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destDir);
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
    }
}