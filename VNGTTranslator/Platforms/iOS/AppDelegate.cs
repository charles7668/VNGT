using Foundation;

namespace VNGTTranslator
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(Environment.GetCommandLineArgs());
    }
}
