namespace VNGTTranslator.Configs
{
    public interface IAppConfigProvider
    {
        AppConfig GetAppConfig();

        bool TrySaveAppConfig(out string errMessage);
    }
}