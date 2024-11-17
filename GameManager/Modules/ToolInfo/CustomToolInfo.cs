using GameManager.Services;
using System.Diagnostics;

namespace GameManager.Modules.ToolInfo
{
    public class CustomToolInfo(string toolName, string toolVersion, string exePath, bool runAsAdmin = false)
        : IToolInfo
    {
        public string ExePath { get; } = exePath;
        public bool RunAsAdmin { get; } = runAsAdmin;
        public string ToolName { get; } = toolName;
        public string ToolVersion { get; } = toolVersion;

        public Task LaunchToolAsync()
        {
            IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
            string exeFullPath = Path.Combine(appPathService.ToolsDirPath, ToolName, ExePath);
            ProcessStartInfo startInfo = new(exeFullPath)
            {
                UseShellExecute = false
            };
            if (RunAsAdmin)
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            Process.Start(startInfo);
            return Task.CompletedTask;
        }
    }
}