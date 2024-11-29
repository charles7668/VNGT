using GameManager.DTOs;
using GameManager.Models;
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

            string executionFile = Path.Combine(gameInfo.ExePath, gameInfo.ExeFile);
            var proc = new Process();
            proc.StartInfo.FileName = executionFile;
            proc.StartInfo.WorkingDirectory = gameInfo.ExePath;
            proc.StartInfo.UseShellExecute = false;
            bool runAsAdmin = gameInfo.LaunchOption is { RunAsAdmin: true };
            if (runAsAdmin)
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
            }

            proc.Start();

            tryLaunchVNGTTranslator?.Invoke(proc.Id);

            return Task.FromResult(proc.Id);
        }
    }
}