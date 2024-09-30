using GameManager.Extractor;
using GameManager.Models;
using GameManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameManager.Components.Pages
{
    public partial class Tools : IDisposable
    {
        private static readonly bool _Is64Bit = !RuntimeInformation.OSArchitecture.HasFlag(Architecture.X86);

        private static readonly BuiltinToolInfo[] _BuiltinToolInfos =
        [
            new BuiltinToolInfo("SavePatcher", "SavePatcher.exe",
                "https://api.github.com/repos/charles7668/VNGT/releases",
                _Is64Bit ? "SavePatcher.x86.7z" : "SavePatcher.x64.7z", "0.4.0.0"),
            new BuiltinToolInfo("VNGTTranslator", "VNGTTranslator.exe",
                "https://api.github.com/repos/charles7668/VNGTTranslator/releases",
                _Is64Bit ? "VNGTTranslator.x86.7z" : "VNGTTranslator.x64.7z", "0.1.1"),
            new BuiltinToolInfo("ProcessTracer", "ProcessTracer.exe",
                "https://api.github.com/repos/charles7668/ProcessTracer/releases",
                _Is64Bit ? "ProcessTracer.x86.7z" : "ProcessTracer.7z", "0.3.0")
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
                CustomToolInfo.OnFailedHandler += OnFailedNotify;
                if (CustomToolInfos != null)
                    return base.OnInitializedAsync();
                CustomToolInfos = [];
                string[] directories = Directory.GetDirectories(AppPathService.ToolsDirPath);
                foreach (string directory in directories)
                {
                    if (_BuiltinToolInfos.Any(x => x.Name == Path.GetFileName(directory)))
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
                        string exePath = configuration.GetValue("ExePath", dirName + ".exe") ?? dirName + ".exe";
                        bool runAsAdmin = configuration.GetValue("RunAsAdmin", false);
                        CustomToolInfo customToolInfo = new(name, exePath, runAsAdmin);
                        CustomToolInfos.Add(customToolInfo);
                    }
                    else
                    {
                        string name = Path.GetFileName(directory);
                        CustomToolInfo customToolInfo = new(name, name + ".exe");
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

        private class CustomToolInfo(string name, string exePath, bool runAsAdmin = false)
        {
            public string Name { get; } = name;

            public static event Action<string, string>? OnFailedHandler;

            public void Launch()
            {
                IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
                string exeFullPath = Path.Combine(appPathService.ToolsDirPath, Name, exePath);
                ProcessStartInfo startInfo = new(exeFullPath)
                {
                    UseShellExecute = false
                };
                if (runAsAdmin)
                {
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = "runas";
                }

                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception e)
                {
                    OnFailed(Name, $"can't launch program : {e.Message}");
                }
            }

            private static void OnFailed(string name, string message)
            {
                OnFailedHandler?.Invoke(name, message);
            }
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
                Logger.LogError(e, "Failed to remove {Name} , {Excep}", name, e.ToString());
                SnakeBar.Add($"Failed to remove {name} : {e.Message}", Severity.Error);
            }
            StateHasChanged();
        }

        private class BuiltinToolInfo(
            string name,
            string exePath,
            string downloadUrl,
            string downloadFileName,
            string? releaseTag = null)
        {
            private static readonly HttpClient _HttpClient = new()
            {
                DefaultRequestHeaders =
                {
                    { "User-Agent", "VNGT" }
                }
            };

            public string Name { get; } = name;
            private string ExePath { get; } = exePath;
            private string DownloadUrl { get; } = downloadUrl;
            private string DownloadFileName { get; } = downloadFileName;
            private Task? DownloadTask { get; set; }
            public int Progress { get; private set; }
            public CancellationTokenSource CancellationTokenSource { get; private set; } = new();

            public bool IsDownloading => DownloadTask != null && DownloadTask != Task.CompletedTask;

            public static event Action<string, int>? OnProgressUpdateHandler;
            public static event Action<string, string>? OnFailedHandler;

            public void Launch()
            {
                IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
                string exeFullPath = Path.Combine(appPathService.ToolsDirPath, Name, ExePath);
                ProcessStartInfo startInfo = new(exeFullPath)
                {
                    UseShellExecute = false
                };
                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception e)
                {
                    OnFailed(Name, $"can't launch program : {e.Message}");
                }
            }

            public void StartDownload()
            {
                CancellationTokenSource = new CancellationTokenSource();
                DownloadTask = Task.Run(async () =>
                {
                    string tempPath = Path.Combine(Path.GetTempPath(), DownloadFileName);
                    CancellationToken token = CancellationTokenSource.Token;
                    string response = await _HttpClient.GetStringAsync(DownloadUrl, token);
                    JsonElement releaseInfos = JsonSerializer.Deserialize<JsonElement>(response);
                    int releaseCount = releaseInfos.GetArrayLength();
                    bool find = false;
                    string? downloadUrl = null;
                    for (int i = 0; i < releaseCount && !find; ++i)
                    {
                        if (releaseTag != null && releaseInfos[i].TryGetProperty("name", out JsonElement title) &&
                            title.GetString() != releaseTag)
                            continue;
                        bool ok = releaseInfos[i].TryGetProperty("assets", out JsonElement assets);
                        if (!ok)
                            continue;
                        foreach (JsonElement asset in assets.EnumerateArray())
                        {
                            if (!asset.TryGetProperty("name", out JsonElement name) ||
                                name.GetString() != DownloadFileName)
                                continue;
                            downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            if (downloadUrl == null)
                            {
                                break;
                            }

                            find = true;
                            break;
                        }
                    }

                    if (find)
                    {
                        using HttpResponseMessage fileResponse =
                            await _HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
                        fileResponse.EnsureSuccessStatusCode();

                        long? totalBytes = fileResponse.Content.Headers.ContentLength;

                        await using Stream contentStream = await fileResponse.Content.ReadAsStreamAsync(token),
                            fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None,
                                8192, true);
                        byte[] buffer = new byte[8192];
                        long totalReadBytes = 0;
                        int readBytes;
                        while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0 &&
                               !token.IsCancellationRequested)
                        {
                            await fileStream.WriteAsync(buffer, 0, readBytes, token);
                            totalReadBytes += readBytes;

                            if (totalBytes.HasValue)
                            {
                                double progress = (double)totalReadBytes / totalBytes.Value * 100;
                                if ((int)Math.Floor(progress) != Progress)
                                {
                                    Progress = (int)Math.Floor(progress);
                                    OnProgressUpdate(Name, Progress);
                                }
                            }
                            else
                            {
                                Progress = 100;
                                OnProgressUpdate(Name, Progress);
                            }
                        }

                        if (token.IsCancellationRequested)
                        {
                            OnFailed(Name, "Task is canceled.");
                            return;
                        }
                    }
                    else
                    {
                        OnFailed(Name, "No download file found");
                        return;
                    }

                    ExtractorFactory extractorFactory = App.ServiceProvider.GetRequiredService<ExtractorFactory>();
                    IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
                    string extension = Path.GetExtension(DownloadFileName);
                    IExtractor? extractor = extractorFactory.GetExtractor(extension);
                    if (extractor == null)
                    {
                        OnFailed(Name, $"can't extract {extension} file");
                        return;
                    }

                    string targetPath = Path.Combine(appPathService.ToolsDirPath, Name);
                    Result<string> extractResult = await extractor.ExtractAsync(tempPath, new ExtractOption
                    {
                        CreateNewFolder = false,
                        TargetPath = targetPath
                    });
                    if (!extractResult.Success)
                    {
                        OnFailed(Name, $"extract failed : {extractResult.Message}");
                        try
                        {
                            Directory.Delete(targetPath);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                }, CancellationTokenSource.Token).ContinueWith(task =>
                {
                    if (task.IsCanceled)
                        OnFailed(Name, "Task is canceled.");
                    Progress = 0;
                    DownloadTask = Task.CompletedTask;
                    OnProgressUpdate(Name, 100);
                });
            }

            private static void OnProgressUpdate(string toolName, int obj)
            {
                OnProgressUpdateHandler?.Invoke(toolName, obj);
            }

            private static void OnFailed(string toolName, string message)
            {
                OnFailedHandler?.Invoke(toolName, message);
            }
        }
    }
}