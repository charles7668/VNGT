using GameManager.Models;
using Helper;
using Ionic.Zip;

namespace GameManager.Extractor
{
    public class ZipExtractor : IExtractor
    {
        public string[] SupportExtensions { get; } = [".zip"];

        public async Task<Result<string>> ExtractAsync(string filePath, ExtractOption option)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string destPath = tempPath;
                if (option.CreateNewFolder)
                    destPath = Path.Combine(destPath, Path.GetFileNameWithoutExtension(filePath));
                var extractTask = Task.Run(() =>
                {
                    using var zip = ZipFile.Read(filePath);
                    if (!string.IsNullOrEmpty(option.Password))
                    {
                        zip.Password = option.Password;
                    }

                    zip.ExtractAll(destPath, ExtractExistingFileAction.OverwriteSilently);
                });
                await extractTask;

                FileHelper.CopyDirectory(tempPath, option.TargetPath);

                return await Task.FromResult(
                    extractTask.IsFaulted
                        ? Result<string>.Failure(extractTask.Exception.Message)
                        : Result<string>.Ok(destPath));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<string>.Failure(ex.Message));
            }
        }
    }
}