using GameManager.DB.Models;
using GameManager.Properties;
using GameManager.Services;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Models.LaunchProgramStrategies
{
    public class LaunchWithSandboxie(GameInfo gameInfo, Action<int>? tryLaunchVNGTTranslator) : IStrategy
    {
        public Task ExecuteAsync()
        {
            if (gameInfo.ExePath == null || gameInfo.ExeFile == null || gameInfo.ExeFile == "Not Set")
            {
                throw new Exception("Execution file not set");
            }

            string executionFile = Path.Combine(gameInfo.ExePath, gameInfo.ExeFile);

            IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
            AppSetting appSetting = configService.GetAppSetting();

            string? sandboxiePath = Path.GetDirectoryName(appSetting.SandboxiePath);
            if (string.IsNullOrEmpty(sandboxiePath))
                throw new Exception("Sandboxie path not set");
            string sandboxieFilePath = Path.Combine(sandboxiePath, "Start.exe");
            string sandboxieBoxName = gameInfo.LaunchOption?.SandboxieBoxName ?? "DefaultBox";
            if (!File.Exists(sandboxieFilePath))
                throw new Exception("Sandboxie Start.exe not found");

            if (gameInfo.LaunchOption?.LaunchWithLocaleEmulator != null &&
                gameInfo.LaunchOption.LaunchWithLocaleEmulator != "None")
            {
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
                string elevate = gameInfo.LaunchOption?.RunAsAdmin ?? false ? "/elevate" : "";

                var proc = new Process();
                proc.StartInfo.FileName = sandboxieFilePath;
                proc.StartInfo.Arguments =
                    $"/Box:{sandboxieBoxName} {elevate} {leExePath} -runas \"{guid}\" \"{executionFile}\"";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
            }
            else
            {
                string elevate = gameInfo.LaunchOption?.RunAsAdmin ?? false ? "/elevate" : "";
                var proc = new Process();
                proc.StartInfo.FileName = sandboxieFilePath;
                proc.StartInfo.Arguments = $"/Box:{sandboxieBoxName} {elevate} \"{executionFile}\"";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
            }

            tryLaunchVNGTTranslator?.Invoke(gameInfo.Id);
            return Task.CompletedTask;
        }
    }
}