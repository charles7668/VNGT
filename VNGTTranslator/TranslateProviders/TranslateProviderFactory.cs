using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNGTTranslator.TranslateProviders
{
    class TranslateProviderFactory
    {
        public static Dictionary<string, ITranslateProvider> CachedProviders { get; }

        [ImportMany]
        private IEnumerable<ITranslateProvider> _supportedTranslateProviders;

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
                Console.WriteLine(compositionException.ToString());
            }
        }
    }
}