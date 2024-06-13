using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace VNGTTranslator.TranslateProviders
{
    internal class TranslateProviderFactory
    {
        public TranslateProviderFactory()
        {
            try
            {
                // An aggregate catalog that combines multiple catalogs.
                var catalog = new AggregateCatalog();
                // Adds all the parts found in the same assembly as the Program class.
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));

                // Create the CompositionContainer with the parts in the catalog.
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
            }

            CachedProviders = SupportedTranslateProviders != null
                ? SupportedTranslateProviders.ToDictionary(p => p.ProviderName)
                : new Dictionary<string, ITranslateProvider>();
        }

        public Dictionary<string, ITranslateProvider> CachedProviders { get; set; }

        [ImportMany]
        private IEnumerable<ITranslateProvider>? SupportedTranslateProviders { get; set; }

        public ITranslateProvider GetProvider(string providerName)
        {
            return CachedProviders[providerName];
        }
    }
}