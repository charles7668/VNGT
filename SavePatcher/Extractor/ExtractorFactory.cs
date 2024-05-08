using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace SavePatcher.Extractor
{
    public class ExtractorFactory
    {
        public ExtractorFactory()
        {
            // An aggregate catalog that combines multiple catalogs.
            AggregateCatalog catalog = new();
            // Adds all the parts found in the same assembly as the Program class.
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));

            // Create the CompositionContainer with the parts in the catalog.
            CompositionContainer container = new(catalog);
            container.ComposeParts(this);

            foreach (IExtractor extractor in _extractors)
            {
                foreach (string extension in extractor.SupportExtensions)
                {
                    _extractorTable[extension] = extractor;
                }
            }
        }

        private readonly Dictionary<string, IExtractor> _extractorTable = [];

        [ImportMany] private IExtractor[] _extractors = [];

        public IExtractor? GetExtractor(string extension)
        {
            return _extractorTable.GetValueOrDefault(extension);
        }
    }
}