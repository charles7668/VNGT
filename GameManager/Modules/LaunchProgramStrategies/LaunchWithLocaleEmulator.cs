using GameManager.DTOs;
using GameManager.Models;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Modules.LaunchProgramStrategies
{
    public class LaunchWithLocaleEmulator(GameInfoDTO gameInfo, Action<int>? tryLaunchVNGTTranslator = null) : IStrategy
    {
        public async Task<int> ExecuteAsync()
        {
            if (gameInfo.ExePath == null || gameInfo.ExeFile == null)
            {
                throw new Exception("Execution file not set");
            }

            string executionFile = Path.Combine(gameInfo.ExePath, gameInfo.ExeFile);

            IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
            AppSettingDTO appSetting = configService.GetAppSettingDTO();
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
            proc.StartInfo.WorkingDirectory = gameInfo.ExePath;
            proc.StartInfo.Arguments = $"-runas \"{guid}\" \"{executionFile}\"";
            proc.StartInfo.UseShellExecute = false;
            bool runAsAdmin = gameInfo.LaunchOption is { RunAsAdmin: true };
            if (runAsAdmin)
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
            }

            proc.Start();
            int leProcId = proc.Id;
            await Task.Delay(100);
            Process? targetProc = ProcessHelper.GetChildProcessesByParentPid(leProcId).FirstOrDefault();

            tryLaunchVNGTTranslator?.Invoke(targetProc?.Id ?? 0);

            return targetProc?.Id ?? proc.Id;
        }
    }
}