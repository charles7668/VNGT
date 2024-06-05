using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;

namespace VNGTTranslator
{
    internal class Program : MauiApplication
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(Environment.GetCommandLineArgs());

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run(args);
        }
    }
}
