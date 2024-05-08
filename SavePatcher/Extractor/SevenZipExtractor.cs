using SavePatcher.Models;
using SevenZip;
using System.ComponentModel.Composition;

namespace SavePatcher.Extractor
{
    [Export(typeof(IExtractor))]
    public class SevenZipExtractor : IExtractor
    {
        public string[] SupportExtensions { get; } = [".7z"];

        public async Task<Result<string>> ExtractAsync(string filePath, ExtractOption option)
        {
            if (!File.Exists(filePath))
            {
                return Result<string>.Failure("File does not exist.");
            }

            try
            {
                string dllPath = Path.Combine(Directory.GetCurrentDirectory(), "7z.dll");
                SevenZipBase.SetLibraryPath(dllPath);
                string destPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                var extractTask = Task.Run(() =>
                {
                    SevenZip.SevenZipExtractor extractor = string.IsNullOrEmpty(option.Password)
                        ? new SevenZip.SevenZipExtractor(filePath)
                        : new SevenZip.SevenZipExtractor(filePath, option.Password);
                    if (option.SpecificFiles.Length > 0)
                    {
                        extractor.ExtractFiles(destPath, option.SpecificFiles);
                    }
                    else
                    {
                        extractor.ExtractArchive(destPath);
                    }
                });
                await extractTask;
                return await Task.FromResult(extractTask.IsFaulted
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