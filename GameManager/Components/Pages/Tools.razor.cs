using GameManager.DB.Models;
using GameManager.Models;
using GameManager.Modules.ToolInfo;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

namespace GameManager.Components.Pages
{
    public partial class Tools : IDisposable
    {
        private static readonly bool _Is64Bit = !RuntimeInformation.OSArchitecture.HasFlag(Architecture.X86);

        private static readonly BuiltinToolInfo[] _BuiltinToolInfos =
        [
            new("SavePatcher", "SavePatcher.exe",
                "https://api.github.com/repos/charles7668/VNGT/releases",
                _Is64Bit ? "SavePatcher.x86.7z" : "SavePatcher.x64.7z", "0.4.0.0"),
            new("VNGTTranslator", "VNGTTranslator.exe",
                "https://api.github.com/repos/charles7668/VNGTTranslator/releases",
                _Is64Bit ? "VNGTTranslator.x86.7z" : "VNGTTranslator.x64.7z", "0.1.1"),
            new("ProcessTracer", "ProcessTracer.exe",
                "https://api.github.com/repos/charles7668/ProcessTracer/releases",
                _Is64Bit ? "ProcessTracer.x86.7z" : "ProcessTracer.7z", "0.4.0"),
            new("Locale-Emulator", "LEGUI.exe",
                "https://api.github.com/repos/xupefei/Locale-Emulator/releases",
                "Locale.Emulator.2.5.0.1.zip", "v2.5.0.1")
            {
                OnDownloadComplete = toolPath =>
                {
                    var assembly = Assembly.LoadFrom(Path.Combine(toolPath, "LEInstaller.exe"));
                    var resourceManager = new ResourceManager("LEInstaller.Properties.Resources", assembly);
                    byte[]? commonLibrary = (byte[]?)resourceManager.GetObject("LECommonLibrary");
                    byte[]? contextMenuHandler = (byte[]?)resourceManager.GetObject("LEContextMenuHandler");
                    if (commonLibrary == null || contextMenuHandler == null)
                        return Result.Failure("Failed to load LECommonLibrary.dll or LEContextMenuHandler.dll");
                    try
                    {
                        File.WriteAllBytes(Path.Combine(toolPath, "LECommonLibrary.dll"), commonLibrary);
                        File.WriteAllBytes(Path.Combine(toolPath, "LEContextMenuHandler.dll"), contextMenuHandler);
                    }
                    catch (Exception)
                    {
                        return Result.Failure("Failed to write LECommonLibrary.dll or LEContextMenuHandler.dll");
                    }

                    IConfigService configService = App.ServiceProvider.GetRequiredService<IConfigService>();
                    AppSetting appSetting = configService.GetAppSetting();
                    if (!string.IsNullOrWhiteSpace(appSetting.LocaleEmulatorPath))
                    {
                        string configLeExePath = Path.Combine(appSetting.LocaleEmulatorPath!, "LEProc.exe");
                        if (File.Exists(configLeExePath))
                            return Result.Ok();
                    }

                    appSetting.LocaleEmulatorPath = toolPath;
                    configService.UpdateAppSettingAsync(appSetting);

                    return Result.Ok();
                }
            }
        ];

        private static List<CustomToolInfo>? CustomToolInfos { get; set; }

        [Inject]
        private ISnackbar SnakeBar { get; set; } = null!;

        [Inject]
        private ILogger<Tools> Logger { get; set; } = null!;

        [Inject]
        private IAppPathService AppPathService { get; set; } = null!;

        public void Dispose()
        {
            SnakeBar.Dispose();
            BuiltinToolInfo.OnProgressUpdateHandler -= OnDownloadProgressUpdate;
            BuiltinToolInfo.OnFailedHandler -= OnFailedNotify;
        }

        protected override Task OnInitializedAsync()
        {
            try
            {
                BuiltinToolInfo.OnProgressUpdateHandler += OnDownloadProgressUpdate;
                BuiltinToolInfo.OnFailedHandler += OnFailedNotify;
                if (CustomToolInfos != null)
                    return base.OnInitializedAsync();
                CustomToolInfos = [];
                string[] directories = Directory.GetDirectories(AppPathService.ToolsDirPath);
                foreach (string directory in directories)
                {
                    if (_BuiltinToolInfos.Any(x => x.ToolName == Path.GetFileName(directory)))
                        continue;
                    string configFile = Path.Combine(directory, "conf.vngt.yaml");
                    if (File.Exists(configFile))
                    {
                        IConfigurationRoot configuration = new ConfigurationBuilder()
                            .SetBasePath(directory)
                            .AddYamlFile("conf.vngt.yaml", false, false)
                            .Build();
                        string dirName = Path.GetFileName(directory);
                        string name = configuration.GetValue("Name", dirName) ?? dirName;
                        string version = configuration.GetValue("Version", "") ?? "";
                        string exePath = configuration.GetValue("ExePath", dirName + ".exe") ?? dirName + ".exe";
                        bool runAsAdmin = configuration.GetValue("RunAsAdmin", false);
                        CustomToolInfo customToolInfo = new(name, version, exePath, runAsAdmin);
                        CustomToolInfos.Add(customToolInfo);
                    }
                    else
                    {
                        string name = Path.GetFileName(directory);
                        CustomToolInfo customToolInfo = new(name, "", name + ".exe");
                        CustomToolInfos.Add(customToolInfo);
                    }
                }

                return base.OnInitializedAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize Tools page {Exception}", ex.ToString());
                throw;
            }
        }

        private void OnOpenToolsFolderClick()
        {
            string toolsFolder = AppPathService.ToolsDirPath;

            try
            {
                Process.Start("explorer.exe", toolsFolder);
            }
            catch (Exception e)
            {
                SnakeBar.Add(e.Message, Severity.Error);
            }
        }

        private void OnDownloadProgressUpdate(string toolName, int progress)
        {
            InvokeAsync(StateHasChanged);
        }

        private void OnFailedNotify(string name, string message)
        {
            InvokeAsync(() =>
            {
                SnakeBar.Add($"{name} error occur : {message}", Severity.Error);
            });
        }

        private void RemoveTool(string name)
        {
            string toolsPath = AppPathService.ToolsDirPath;
            string toolPath = Path.Combine(toolsPath, name);
            if (!Directory.Exists(toolPath)) return;
            try
            {
                Directory.Delete(toolPath, true);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to remove {Name} , {Except}", name, e.ToString());
                SnakeBar.Add($"Failed to remove {name} : {e.Message}", Severity.Error);
            }

            StateHasChanged();
        }

        private async Task LaunchToolAsync(IToolInfo toolInfo)
        {
            try
            {
                await toolInfo.LaunchToolAsync();
            }
            catch (Exception e)
            {
                OnFailedNotify(toolInfo.ToolName, e.Message);
            }
        }
    }
}