using Ionic.Zip;
using SavePatcher.Models;
using System.ComponentModel.Composition;

namespace SavePatcher.Extractor
{
    [Export(typeof(IExtractor))]
    public class ZipExtractor : IExtractor
    {
        public string[] SupportExtensions { get; } = [".zip"];

        public async Task<Result<string>> ExtractAsync(string filePath, ExtractOption option)
        {
            try
            {
                string destPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                var extractTask = Task.Run(() =>
                {
                    using var zip = ZipFile.Read(filePath);
                    if (!string.IsNullOrEmpty(option.Password))
                    {
                        zip.Password = option.Password;
                    }

                    if (option.SpecificFiles.Length > 0)
                    {
                        foreach (string file in option.SpecificFiles)
                        {
                            var zipEntry = zip.FirstOrDefault(entry => entry.FileName.Replace('/', '\\') == file);
                            if (zipEntry == null)
                            {
                                throw new ArgumentException($"File {file} not found in zip.");
                            }

                            zipEntry.Extract(destPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    else
                    {
                        zip.ExtractAll(destPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                });
                await extractTask;

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