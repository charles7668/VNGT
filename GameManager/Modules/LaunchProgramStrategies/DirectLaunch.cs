using GameManager.DTOs;
using GameManager.Models;
using GameManager.Services;
using Helper;
using System.Diagnostics;

namespace GameManager.Modules.LaunchProgramStrategies
{
    public class DirectLaunch(GameInfoDTO gameInfo, Action<int>? tryLaunchVNGTTranslator = null) : IStrategy
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
            executionFile = executionFile.ToUnixPath()!;
            List<string> args = ["-f\"" + executionFile + "\"", "--hide"];
            var proc = new Process();
            proc.StartInfo.FileName = processTracerPath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            bool runAsAdmin = gameInfo.LaunchOption is { RunAsAdmin: true };
            if (runAsAdmin)
            {
                args.Add("--runas");
            }

            proc.StartInfo.Arguments = string.Join(" ", args);

            proc.Start();

            tryLaunchVNGTTranslator?.Invoke(proc.Id);

            return Task.FromResult(proc.Id);
        }
    }
}