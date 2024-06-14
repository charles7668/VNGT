using VNGTTranslator.Models;

namespace VNGTTranslator.Configs
{
    public interface IAppConfigProvider
    {
        /// <summary>
        /// Retrieves the current application configuration instance.
        /// </summary>
        /// <returns>The AppConfig instance containing the application settings.</returns>
        AppConfig GetAppConfig();

        /// <summary>
        /// Tries to save the application configuration to file.
        /// </summary>
        /// <returns>True if the save operation is successful, otherwise false.</returns>
        Result TrySaveAppConfig();

        /// <summary>
        /// Retrieves the configuration for a specific translator provider, stored as key-value pairs in a dictionary.
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns>
        /// if no configuration is found, an empty dictionary is returned.
        /// </returns>
        public Dictionary<string, object> GetTranslatorProviderConfig(string providerName);

        /// <summary>
        /// save the configuration for a specific translator provider.
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="config"></param>
        public Result SaveTranslatorProviderConfig(string providerName, Dictionary<string, object> config);
    }
}