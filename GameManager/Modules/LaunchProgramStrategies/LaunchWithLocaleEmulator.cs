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
        public Task<int> ExecuteAsync()
        {
            if (gameInfo.ExePath == null || gameInfo.ExeFile == null)
            {
                throw new Exception("Execution file not set");
            }

            IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
            string processTracerPath = Path.Combine(appPathService.ProcessTracerDirPath, "ProcessTracer.exe");
            if (string.IsNullOrEmpty(processTracerPath))
            {
                throw new FileNotFoundException("ProcessTracer not found");
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
            List<string> args = [$"-f\"{leExePath}\"" , $"-a\"-runas {guid} \\\"{executionFile}\\\"\"", "--hide"];
            proc.StartInfo.FileName = processTracerPath;
            proc.StartInfo.WorkingDirectory = gameInfo.ExePath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            bool runAsAdmin = gameInfo.LaunchOption is { RunAsAdmin: true };
            if (runAsAdmin)
            {
                args.Add("--runas");
            }
            proc.StartInfo.Arguments = string.Join(" ", args);

            proc.Start();

            return Task.FromResult(proc.Id);
        }
    }
}