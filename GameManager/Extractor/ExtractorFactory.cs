namespace GameManager.Extractor
{
    public class ExtractorFactory
    {
        public ExtractorFactory(IEnumerable<IExtractor> extractors)
        {
            foreach (IExtractor extractor in extractors)
            {
                foreach (string extractorSupportExtension in extractor.SupportExtensions)
                {
                    _extractors.TryAdd(extractorSupportExtension, extractor);
                }
            }

            SupportedExtensions = _extractors.Keys.ToList();
        }

        private readonly Dictionary<string, IExtractor> _extractors = [];

        public IReadOnlyList<string> SupportedExtensions { get; private set; }

        public IExtractor? GetExtractor(string extension)
        {
            return _extractors.TryGetValue(extension, out IExtractor? extractor) ? extractor : null;
        }
    }
}