using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text.Json;
using VNGTTranslator.Models;

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
                _translateProviderConfigCache = new MemoryCache(new MemoryCacheOptions
                {
                    SizeLimit = 20
                });
            }
            else
            {
                _appConfig = new AppConfig();
                _translateProviderConfigCache = new MemoryCache(new MemoryCacheOptions());
            }

            _appConfigPath = appConfigPath;
        }

        private readonly AppConfig _appConfig;

        private readonly string _appConfigPath;

        private readonly IMemoryCache _translateProviderConfigCache;

        public AppConfig GetAppConfig()
        {
            return _appConfig;
        }

        public Dictionary<string, object> GetTranslatorProviderConfig(string providerName)
        {
            if (_translateProviderConfigCache.TryGetValue(providerName, out Dictionary<string, object>? config))
            {
                return config!;
            }

            if (string.IsNullOrEmpty(_appConfigPath))
            {
                return new Dictionary<string, object>();
            }

            string? configFolder = Path.GetDirectoryName(_appConfigPath);
            if (configFolder == null)
                return new Dictionary<string, object>();
            string configFile = Path.Combine(configFolder, "translator", $"{providerName}.json");
            if (!File.Exists(configFile))
                return new Dictionary<string, object>();
            string jsonString = File.ReadAllText(configFile);
            config = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
            if (config == null)
                return new Dictionary<string, object>();
            _translateProviderConfigCache.Set(providerName, config);
            return config;
        }

        public Result SaveTranslatorProviderConfig(string providerName, Dictionary<string, object> config)
        {
            if (string.IsNullOrEmpty(_appConfigPath))
            {
                _translateProviderConfigCache.Set(providerName, config);
            }
            else
            {
                try
                {
                    string configFolder = Path.GetDirectoryName(_appConfigPath)!;
                    string configFile = Path.Combine(configFolder, "translator", $"{providerName}.json");
                    string writeString = JsonSerializer.Serialize(config);
                    File.WriteAllText(configFile, writeString);
                    _translateProviderConfigCache.Set(providerName, config);
                }
                catch (Exception e)
                {
                    return Result.Fail(e.Message);
                }
            }

            return Result.Success();
        }

        public Result TrySaveAppConfig()
        {
            if (string.IsNullOrEmpty(_appConfigPath))
            {
                return Result.Success();
            }

            try
            {
                string jsonString = JsonSerializer.Serialize(_appConfig);
                File.WriteAllText(_appConfigPath, jsonString);
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
    }
}