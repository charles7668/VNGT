using System.IO;
using System.Text.Json;

namespace VNGTTranslator.Configs
{
    internal class AppConfigProvider : IAppConfigProvider
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="appConfigPath">config file path , if empty then store in memory</param>
        /// <exception cref="JsonException"></exception>
        public AppConfigProvider(string appConfigPath)
        {
            if (!string.IsNullOrEmpty(appConfigPath))
            {
                string jsonString = File.ReadAllText(appConfigPath);
                AppConfig? config = JsonSerializer.Deserialize<AppConfig>(jsonString);
                _appConfig = config ?? throw new JsonException("Invalid app config file");
            }
            else
            {
                _appConfig = new AppConfig();
            }

            _appConfigPath = appConfigPath;
        }

        private readonly AppConfig _appConfig;

        private readonly string _appConfigPath;

        public AppConfig GetAppConfig()
        {
            return _appConfig;
        }

        public bool TrySaveAppConfig(out string errMessage)
        {
            if (string.IsNullOrEmpty(_appConfigPath))
            {
                errMessage = string.Empty;
                return true;
            }

            try
            {
                string jsonString = JsonSerializer.Serialize(_appConfig);
                File.WriteAllText(_appConfigPath, jsonString);
                errMessage = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
    }
}