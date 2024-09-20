using GameManager.Extractor;

namespace GameManager.Extensions
{
    internal static class ServiceCollectionExtension
    {
        public static IServiceCollection AddExtractors(this IServiceCollection services)
        {
            services.AddSingleton<IExtractor, SevenZipExtractor>();

            services.AddSingleton<ExtractorFactory, ExtractorFactory>();

            return services;
        }
    }
}