using System.ComponentModel.Composition;

namespace VNGTTranslator.TranslateProviders.Microsoft
{
    [Export(typeof(ITranslateProvider))]
    internal class MicrosoftTranslateProvider : ITranslateProvider
    {
        public string ProviderName { get; } = "Microsoft";
        public Task<string> TranslateAsync(string text, LanguageConstant.Language sourceLanguage, LanguageConstant.Language targetLanguage)
        {
            throw new NotImplementedException();
        }
    }
}