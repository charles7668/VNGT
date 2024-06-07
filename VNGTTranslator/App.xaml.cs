using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using VNGTTranslator.LunaHook;

namespace VNGTTranslator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
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
            Program.Is64Bit = Environment.Is64BitProcess;

            void Log(string text)
            {
                Debugger.Log(1, "", text);
            }

            File.WriteAllText("test.txt", "");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var cp932 = Encoding.GetEncoding(932);

            // start luna hook service
            HookMethod.LunaStart(inPid =>
            {
                File.AppendAllText("test.txt", $"connected {inPid}\n");
            }, inPid =>
            {
                File.AppendAllText("test.txt", $"disconnected {inPid}\n");
            }, (code, name, tp) =>
            {
                File.AppendAllText("test.txt", $"create {code}\n");
            }, (code, name, tp) =>
            {
                File.AppendAllText("test.txt", $"destroy {code}\n");
            }, (hookCode, name, tp, output) =>
            {
                string write = cp932.GetString(cp932.GetBytes(output));
                File.AppendAllText("test.txt", $"output {hookCode} {write}\n");
                return false;
            }, output =>
            {
                string write = cp932.GetString(cp932.GetBytes(output));
                File.AppendAllText("test.txt", $"console {write}\n");
            }, (address, output) =>
            {
                File.AppendAllText("test.txt", $"hookInsert {address} {output}\n");
            }, (output, tp) =>
            {
            });

            HookMethod.LunaSettings(100, false, 932, 3000, 10000000);

            string lunaHookPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LunaHook"));

            try
            {
                HookMethod.LunaDetach(Program.PID);
            }
            catch (Exception)
            {
                // ignored
            }

            HookMethod.LunaInject(pid, lunaHookPath);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                HookMethod.LunaDetach(Program.PID);
            }
            catch (Exception)
            {
                // ignored
            }

            base.OnExit(e);
        }
    }
}