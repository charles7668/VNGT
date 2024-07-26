namespace GameManager.GameInfoProvider
{
    internal class GameInfoProviderFactory
    {
        public GameInfoProviderFactory(IEnumerable<IGameInfoProvider> providers)
        {
            foreach (IGameInfoProvider gameInfoProvider in providers)
            {
                _providers.Add(gameInfoProvider.ProviderName, gameInfoProvider);
            }
        }

        private readonly Dictionary<string, IGameInfoProvider> _providers = [];

        public IGameInfoProvider? GetProvider(string providerName)
        {
            _providers.TryGetValue(providerName, out IGameInfoProvider? provider);
            return provider;
        }
    }
}