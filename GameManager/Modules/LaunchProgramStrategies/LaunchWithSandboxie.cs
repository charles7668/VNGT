using GameManager.DTOs;
using GameManager.Models;
using GameManager.Properties;
using GameManager.Services;
using Helper.Models;
using Helper.Sandboxie;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Modules.LaunchProgramStrategies
{
    public class LaunchWithSandboxie(GameInfoDTO gameInfo, Action<int>? tryLaunchVNGTTranslator) : IStrategy
    {
        public async Task<int> ExecuteAsync()
        {
            if (gameInfo.ExePath == null || gameInfo.ExeFile == null || gameInfo.ExeFile == "Not Set")
            {
                throw new Exception("Execution file not set");
            }

            string executionFile = Path.Combine(gameInfo.ExePath, gameInfo.ExeFile);

            IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
            AppSettingDTO appSetting = configService.GetAppSettingDTO();

            string? sandboxiePath = Path.GetDirectoryName(appSetting.SandboxiePath);
            if (string.IsNullOrEmpty(sandboxiePath))
                throw new Exception("Sandboxie path not set");
            string sandboxieFilePath = Path.Combine(sandboxiePath, "Start.exe");
            string sandboxieBoxName = gameInfo.LaunchOption?.SandboxieBoxName ?? "DefaultBox";
            if (!File.Exists(sandboxieFilePath))
                throw new Exception("Sandboxie Start.exe not found");

            int resultPid;
            var processesBeforeStart = SandboxieHelper.GetSandboxieProcesses(sandboxieFilePath, sandboxieBoxName)
                .Select(x => x.Id).ToHashSet();

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
                proc.StartInfo.WorkingDirectory = gameInfo.ExePath;
                proc.StartInfo.Arguments =
                    $"/Box:{sandboxieBoxName} {elevate} {leExePath} -runas \"{guid}\" \"{executionFile}\"";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                resultPid = proc.Id;
            }
            else
            {
                string elevate = gameInfo.LaunchOption?.RunAsAdmin ?? false ? "/elevate" : "";
                var proc = new Process();
                proc.StartInfo.FileName = sandboxieFilePath;
                proc.StartInfo.WorkingDirectory = gameInfo.ExePath;
                proc.StartInfo.Arguments = $"/Box:{sandboxieBoxName} {elevate} \"{executionFile}\"";
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                resultPid = proc.Id;
            }

            var sw = new Stopwatch();
            sw.Start();
            bool isFind = false;
            // Detect start is used to check if the Sandboxie start.exe is running
            // because the UAC prompt will keep start.exe alive
            bool detectedStart = false;
            while (detectedStart || (!isFind && sw.Elapsed.TotalSeconds < 5))
            {
                detectedStart = false;
                isFind = false;
                List<ProcessInfo> processesAfterStart = [];
                await Task.Run(() =>
                {
                    processesAfterStart =
                        SandboxieHelper.GetSandboxieProcesses(sandboxieFilePath, sandboxieBoxName);
                });
                processesAfterStart = processesAfterStart.Where(x => !processesBeforeStart.Contains(x.Id)).ToList();
                foreach (ProcessInfo p in processesAfterStart)
                {
                    if (p.ExecutablePath == executionFile)
                    {
                        resultPid = p.Id;
                        isFind = true;
                        break;
                    }

                    if (p.ExecutablePath != sandboxieFilePath)
                        continue;
                    detectedStart = true;
                    sw.Restart();
                }

                if (!isFind)
                    await Task.Delay(50);
            }

            tryLaunchVNGTTranslator?.Invoke(gameInfo.Id);
            return resultPid;
        }
    }
}