using Helper;
using Helper.Web;
using Microsoft.Extensions.Caching.Memory;
using SavePatcher.Extractor;
using SavePatcher.Logs;
using SavePatcher.Models;
using System.Text.RegularExpressions;

namespace SavePatcher.Patcher
{
    public class SavePatcher(ExtractorFactory extractorFactory) : IPatcher
    {
        /// <summary>
        /// cache url file
        /// </summary>
        private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 100 });

        /// <summary>
        /// save file path , can use http or local file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// if file is zip , password to extract zip
        /// </summary>
        public string ZipPassword { get; set; } = string.Empty;

        /// <summary>
        /// file list to specify which files in zip to patch
        /// </summary>
        public string[] PatchFiles { get; set; } = [];

        /// <summary>
        /// destination path , if the destination path is left empty, it will be selected manually
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// log callback events
        /// </summary>
        public LogCallbacks? LogCallbacks { get; set; }

        /// <inheritdoc />
        public Result Patch()
        {
            Task<Result> patchTask = PatchAsync();
            patchTask.Wait();
            return patchTask.Result;
        }

        /// <inheritdoc />
        public async Task<Result> PatchAsync()
        {
            string errMessage;
            LogCallbacks?.OnLogInfo(this, $"start check save file {FilePath} exist...");
            string filePath = FilePath;
            if (filePath.IsHttpLink())
            {
                LogCallbacks?.OnLogInfo(this, "download file...");
                if (_cache.TryGetValue(filePath, out string? cachePath)
                    && !string.IsNullOrEmpty(cachePath)
                    && File.Exists(cachePath))
                {
                    filePath = cachePath;
                }
                else
                {
                    var result = await FileDownloader.DownloadFileAsync(filePath,
                        Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                    if (!result.success)
                    {
                        LogCallbacks?.OnLogError(this, result.message);
                        return Result.Failure(result.message);
                    }

                    _cache.Set(filePath, result.message, new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                        .SetSize(1));

                    filePath = result.message;
                }

                LogCallbacks?.OnLogInfo(this, "download complete");
            }
            else if (!File.Exists(filePath))
            {
                errMessage = "file not exist";
                LogCallbacks?.OnLogError(this, errMessage);
                return Result.Failure(errMessage);
            }

            if (!File.Exists(filePath))
            {
                errMessage = "file not exist";
                LogCallbacks?.OnLogError(this, errMessage);
                return Result.Failure(errMessage);
            }

            LogCallbacks?.OnLogInfo(this, "file check success");
            LogCallbacks?.OnLogInfo(this, "start extract file...");
            string extension = Path.GetExtension(filePath);
            var extractor = extractorFactory.GetExtractor(extension);
            if (extractor == null)
            {
                errMessage = $"extractor for extension : '{extension}' not found";
                LogCallbacks?.OnLogError(this, errMessage);
                return Result.Failure(errMessage);
            }

            Result<string> extractResult = await extractor.ExtractAsync(filePath,
                new ExtractOption { Password = ZipPassword, SpecificFiles = PatchFiles });

            if (!extractResult.Success || extractResult.Value == null)
            {
                LogCallbacks?.OnLogError(this, extractResult.Message);
                return Result.Failure(extractResult.Message);
            }

            if (PatchFiles.Length == 0)
            {
                LogCallbacks?.OnLogInfo(this, "extract all file success");
            }
            else
            {
                foreach (string configPatchFile in PatchFiles)
                {
                    LogCallbacks?.OnLogInfo(this, $"extract {configPatchFile} success");
                }
            }

            string destinationPath = DestinationPath;
            if (string.IsNullOrEmpty(destinationPath))
            {
                var folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    destinationPath = folderBrowserDialog.SelectedPath;
                }
                else
                {
                    errMessage = "patch canceled";
                    LogCallbacks?.OnLogError(this, errMessage);
                    return Result.Failure(errMessage);
                }
            }
            else
            {
                string pattern = @"%([^%]+)%"; 
                var matches = Regex.Matches(destinationPath, pattern);

                foreach (Match match in matches)
                {
                    var realPath = Environment.GetEnvironmentVariable(match.Groups[1].Value);
                    destinationPath = destinationPath.Replace(match.Value, realPath);
                }
            }

            if (!Directory.Exists(destinationPath))
            {
                errMessage = "destination path not exist";
                LogCallbacks?.OnLogError(this, errMessage);
                return Result.Failure(errMessage);
            }

            LogCallbacks?.OnLogInfo(this, $"start copy to {destinationPath}");
            try
            {
                if (PatchFiles.Length > 0)
                {
                    foreach (string configPatchFile in PatchFiles)
                    {
                        File.Copy(Path.Combine(extractResult.Value, configPatchFile),
                            Path.Combine(destinationPath, Path.GetFileName(configPatchFile)), true);
                    }
                }
                else
                {
                    FileHelper.CopyDirectory(extractResult.Value, destinationPath);
                }
            }
            catch (Exception ex)
            {
                LogCallbacks?.OnLogError(this, ex.Message);
                return Result.Failure(ex.Message);
            }

            LogCallbacks?.OnLogInfo(this, "patch complete");
            return Result.Ok();
        }
    }
}