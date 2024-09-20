﻿using GameManager.Models;
using Helper;
using SevenZip;

namespace GameManager.Extractor
{
    public class SevenZipExtractor : IExtractor
    {
        public string[] SupportExtensions { get; } = [".7z" , ".rar" , ".001" , ".zip"];

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
                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string destPath = tempPath;
                if (option.CreateNewFolder)
                    destPath = Path.Combine(destPath, Path.GetFileNameWithoutExtension(filePath));
                var extractTask = Task.Run(async () =>
                {
                    SevenZip.SevenZipExtractor extractor = string.IsNullOrEmpty(option.Password)
                        ? new SevenZip.SevenZipExtractor(filePath)
                        : new SevenZip.SevenZipExtractor(filePath, option.Password);
                    if (option.ProgressChanged != null)
                        extractor.Extracting += (sender, args) =>
                            option.ProgressChanged.Invoke(sender, args.PercentDone);
                    bool finish = false;
                    extractor.ExtractionFinished += (_, _) => finish = true;
                    extractor.BeginExtractArchive(destPath);
                    while (!finish)
                    {
                        await Task.Delay(10);
                    }
                });
                await extractTask;

                FileHelper.CopyDirectory(tempPath, option.TargetPath);

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