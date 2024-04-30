using Microsoft.Extensions.DependencyInjection;
using SavePatcher.Configs;
using SavePatcher.Extractor;
using System.Diagnostics;

namespace SavePatcher
{
    internal static class Program
    {
        /// <summary>
        /// Service provider for program.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// configs path
        /// </summary>
        public static string ConfigPath => "configs";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // register services
            ServiceCollection services = new();
            services.AddSingleton<ExtractorFactory>();
            services.AddSingleton(typeof(IConfigReader<>), typeof(YamlConfigReader<>));
            services.AddTransient<Patcher.SavePatcher, Patcher.SavePatcher>();
            ServiceProvider = services.BuildServiceProvider();
            Debug.Assert(ServiceProvider != null);

            // Create necessary directories
            Directory.CreateDirectory(ConfigPath);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }
    }
}