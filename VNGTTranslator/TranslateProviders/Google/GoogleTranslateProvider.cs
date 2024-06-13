using System.ComponentModel.Composition;

namespace VNGTTranslator.TranslateProviders.Google
{
    [Export(typeof(ITranslateProvider))]
    public class GoogleTranslateProvider : ITranslateProvider
    {
        public string ProviderName { get; } = "Google";
    }
}