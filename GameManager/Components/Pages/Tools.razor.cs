using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;

namespace GameManager.Components.Pages
{
    public partial class Tools
    {
        [Inject]
        private ISnackbar SnakeBar { get; set; } = null!;

        public void OnPatcherClick()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string toolFile = Path.Combine(appPath, "tools", "SavePatcher", "SavePatcher.exe");
            if (!File.Exists(toolFile))
            {
                SnakeBar.Add("SavePatcher not exist", Severity.Warning);
                return;
            }

            var startInfo = new ProcessStartInfo(toolFile)
            {
                UseShellExecute = false
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                SnakeBar.Add(e.Message, Severity.Error);
            }
        }

        private void OnOpenToolsFolderClick()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string toolsFolder = Path.Combine(appPath, "tools");
            Directory.CreateDirectory(toolsFolder);

            try
            {
                Process.Start("explorer.exe", toolsFolder);
            }
            catch (Exception e)
            {
                SnakeBar.Add(e.Message, Severity.Error);
            }
        }
    }
}