namespace VNGTTranslator.TranslateProviders
{
    public interface ITranslateProvider
    {
        string ProviderName { get; }

        public Task<string> TranslateAsync(string text, LanguageConstant.Language sourceLanguage,
            LanguageConstant.Language targetLanguage);
    }
}