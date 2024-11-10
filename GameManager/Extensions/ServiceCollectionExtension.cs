using GameManager.Extractor;
using GameManager.GameInfoProvider;
using GameManager.Models.SecurityProvider;
using GameManager.Models.Synchronizer;
using GameManager.Models.Synchronizer.Drivers;
using GameManager.Models.TaskManager;
using GameManager.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        public static IServiceCollection AddGameInfoProviders(this IServiceCollection services)
        {
            services.TryAddScoped<IWebService, WebService>();
            services.AddScoped<IGameInfoProvider, VndbProvider>();
            services.AddScoped<IGameInfoProvider, DLSiteProvider>();
            services.AddScoped<IGameInfoProvider, YmgalInfoProvider>();
            services.AddScoped<GameInfoProviderFactory, GameInfoProviderFactory>();

            return services;
        }

        public static IServiceCollection AddSynchronizers(this IServiceCollection services)
        {
            services.TryAddSingleton<ISecurityProvider, AESSecurityProvider>();
            services.TryAddSingleton<ITaskManager, TaskManager>();
            services.AddSingleton<IWebDAVDriver, WebDAVWebDAVDriver>();
            services.AddSingleton<ISynchronizer, Synchronizer>();

            return services;
        }
    }
}