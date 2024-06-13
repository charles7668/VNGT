using HandyControl.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using VNGTTranslator.Hooker;
using VNGTTranslator.LunaHook;

namespace VNGTTranslator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHooker _hooker = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            Program.InitServices();

            ConfigHelper.Instance.SetLang("en");

            base.OnStartup(e);

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length < 2 || !uint.TryParse(commandLineArgs[1], out uint pid))
            {
                MessageBox.Show("Please provide process pid", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
                return;
            }

            Program.PID = pid;

            _hooker = Program.ServiceProvider.GetRequiredService<IHooker>();
            _hooker.Start();
            try
            {
                _hooker.Detach(pid);
            }
            catch (Exception)
            {
                // ignore
            }

            try
            {
                _hooker.Inject(pid, LunaDll.LunaHookDllPath);
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Inject failed : {exc.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _hooker.Detach(Program.PID);
            }
            catch (Exception)
            {
                // ignored
            }

            base.OnExit(e);
        }
    }
}