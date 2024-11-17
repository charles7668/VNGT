using GameManager.Extractor;
using GameManager.Models;
using GameManager.Services;
using System.Text.Json;

namespace GameManager.Modules.ToolInfo
{
    public class BuiltinToolInfo(
        string toolName,
        string exePath,
        string downloadUrl,
        string downloadFileName,
        string releaseTag = "")
        : CustomToolInfo(toolName, releaseTag, exePath)
    {
        private static readonly HttpClient _HttpClient = new()
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "VNGT" }
            }
        };

        private readonly string _releaseTag = releaseTag;

        private string DownloadUrl { get; } = downloadUrl;
        private string DownloadFileName { get; } = downloadFileName;
        private Task? DownloadTask { get; set; }
        public int Progress { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; } = new();

        public bool IsDownloading => DownloadTask != null && DownloadTask != Task.CompletedTask;
        public Func<string, Result>? OnDownloadComplete { get; set; }

        public static event Action<string, int>? OnProgressUpdateHandler;
        public static event Action<string, string>? OnFailedHandler;

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
                    if (_releaseTag != null && releaseInfos[i].TryGetProperty("name", out JsonElement title) &&
                        title.GetString() != _releaseTag)
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
                            if ((int)Math.Floor(progress) == Progress) continue;
                            Progress = (int)Math.Floor(progress);
                        }
                        else
                        {
                            Progress = 100;
                        }

                        OnProgressUpdate(ToolName, Progress);
                    }

                    if (token.IsCancellationRequested)
                    {
                        OnFailed(ToolName, "Task is canceled.");
                        return;
                    }
                }
                else
                {
                    OnFailed(ToolName, "No download file found");
                    return;
                }

                ExtractorFactory extractorFactory = App.ServiceProvider.GetRequiredService<ExtractorFactory>();
                IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
                string extension = Path.GetExtension(DownloadFileName);
                IExtractor? extractor = extractorFactory.GetExtractor(extension);
                if (extractor == null)
                {
                    OnFailed(ToolName, $"can't extract {extension} file");
                    return;
                }

                string targetPath = Path.Combine(appPathService.ToolsDirPath, ToolName);
                Result<string> extractResult = await extractor.ExtractAsync(tempPath, new ExtractOption
                {
                    CreateNewFolder = false,
                    TargetPath = targetPath
                });
                if (!extractResult.Success)
                {
                    OnFailed(ToolName, $"extract failed : {extractResult.Message}");
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
                {
                    OnFailed(ToolName, "Task is canceled.");
                    return;
                }

                IAppPathService appPathService = App.ServiceProvider.GetRequiredService<IAppPathService>();
                Result? isCompleteTaskSuccess =
                    OnDownloadComplete?.Invoke(Path.Combine(appPathService.ToolsDirPath, ToolName));
                if (isCompleteTaskSuccess is { Success: false })
                {
                    OnFailed(ToolName, isCompleteTaskSuccess.Message);
                    return;
                }

                Progress = 0;
                DownloadTask = Task.CompletedTask;
                OnProgressUpdate(ToolName, 100);

                // write info to config file
                string confPath = Path.Combine(appPathService.ToolsDirPath, ToolName, "conf.vngt.yaml");
                using StreamWriter writer = new(confPath);
                writer.WriteLine("Name: " + ToolName);
                writer.WriteLine("Version: " + ToolVersion);
                writer.WriteLine("ExePath: " + ExePath);
                writer.WriteLine("RunAsAdmin: false");
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