using GameManager.DB.Models;
using GameManager.Properties;
using GameManager.Services;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Models.LaunchProgramStrategies
{
    public class LaunchWithLocaleEmulator(GameInfo gameInfo, Action<int>? tryLaunchVNGTTranslator = null) : IStrategy
    {
        public async Task ExecuteAsync()
        {
            if (gameInfo.ExePath == null || gameInfo.ExeFile == null)
            {
                throw new Exception("Execution file not set");
            }

            string executionFile = Path.Combine(gameInfo.ExePath, gameInfo.ExeFile);

            IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
            AppSetting appSetting = configService.GetAppSetting();
            string leConfigPath = Path.Combine(appSetting.LocaleEmulatorPath!, "LEConfig.xml");
            if (!File.Exists(leConfigPath))
            {
                throw new Exception(Resources.Message_LENotFound);
            }

            var xmlDoc = XDocument.Load(leConfigPath);
            XElement? node =
                xmlDoc.XPathSelectElement(
                    $"//Profiles/Profile[@Name='{gameInfo.LaunchOption?.LaunchWithLocaleEmulator}']");
            XAttribute? guidAttr = node?.Attribute("Guid");
            if (guidAttr == null)
            {
                throw new Exception(
                    $"LE Config {gameInfo.LaunchOption!.LaunchWithLocaleEmulator} {Resources.Message_NotExist}");
            }

            string guid = guidAttr.Value;
            string leExePath = Path.Combine(appSetting.LocaleEmulatorPath!, "LEProc.exe");
            var proc = new Process();
            proc.StartInfo.FileName = leExePath;
            proc.StartInfo.Arguments = $"-runas \"{guid}\" \"{executionFile}\"";
            proc.StartInfo.UseShellExecute = false;
            bool runAsAdmin = gameInfo.LaunchOption is { RunAsAdmin: true };
            if (runAsAdmin)
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
            }

            proc.Start();

            await Task.Delay(500);

            // this process search need run program with privilege
            Process[] processes = Process.GetProcesses();
            int foundPid = 0;
            foreach (Process process in processes)
            {
                try
                {
                    if (string.Equals(process.MainModule?.FileName, executionFile,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        foundPid = process.Id;
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            tryLaunchVNGTTranslator?.Invoke(foundPid);

            DateTime time = DateTime.UtcNow;
            await configService.UpdateLastPlayedByIdAsync(gameInfo.Id, time);
            gameInfo.LastPlayed = time;
        }
    }
}