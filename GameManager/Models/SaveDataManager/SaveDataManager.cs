using GameManager.DB.Models;
using GameManager.Extractor;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace GameManager.Models.SaveDataManager
{
    public class SaveDataManager(ILogger<SaveDataManager> logger, IAppPathService appPathService) : ISaveDataManager
    {
        private const int MAX_BACKUP_COUNT = 10;

        public int MaxBackupCount => MAX_BACKUP_COUNT;

        public Task<List<string>> GetBackupListAsync(GameInfo gameInfo)
        {
            string backupPath = gameInfo.GameUniqueId.ToString();
            string backupDirPath = Path.Combine(appPathService.SaveFileBackupDirPath, backupPath);
            if (!Directory.Exists(backupDirPath))
            {
                return Task.FromResult(new List<string>());
            }

            IEnumerable<string> files = Directory.EnumerateFiles(backupDirPath).OrderByDescending(x => x)
                .Select(Path.GetFileNameWithoutExtension).Take(MAX_BACKUP_COUNT)!;
            return Task.FromResult(files.ToList());
        }

        public Task<Result<string>> BackupSaveFileAsync(GameInfo gameInfo)
        {
            logger.LogInformation("Start backup save file for Game : {GameName} UniqueId : {UniqueId}",
                gameInfo.GameName,
                gameInfo.GameUniqueId);
            string? saveFilePath = gameInfo.SaveFilePath;
            logger.LogInformation("SaveFilePath : {SaveFilePath}", saveFilePath);
            if (string.IsNullOrWhiteSpace(saveFilePath))
            {
                logger.LogWarning("{GameInfoSaveFilePathName} : {MessageParameterNotSet}",
                    nameof(saveFilePath), Resources.Message_ParameterNotSet);
                return Task.FromResult(
                    Result<string>.Failure($"{nameof(saveFilePath)} : {Resources.Message_ParameterNotSet}"));
            }

            if (!Directory.Exists(saveFilePath))
            {
                logger.LogWarning("{GameInfoSaveFilePathName} : {MessageDirectoryNotExist}",
                    nameof(saveFilePath), Resources.Message_DirectoryNotExist);
                return Task.FromResult(Result<string>.Failure(
                    $"{nameof(saveFilePath)} : {Resources.Message_DirectoryNotExist}"));
            }

            string backupDir = Path.Combine(appPathService.SaveFileBackupDirPath, gameInfo.GameUniqueId.ToString());
            Directory.CreateDirectory(backupDir);
            // remove old backup files
            IEnumerable<string> oldFiles = Directory.EnumerateFiles(backupDir, "*.zip")
                .OrderByDescending(x => x).Skip(MAX_BACKUP_COUNT - 1);
            foreach (string file in oldFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Delete old backup file failed : {File}", file);
                }
            }

            string targetPath = Path.Combine(backupDir, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.zip");
            using ZipArchive zipArchive = ZipFile.Open(targetPath, ZipArchiveMode.Create);
            string[] files = Directory.GetFiles(saveFilePath, "*", SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                string relativePath = Path.GetRelativePath(saveFilePath, filePath);
                zipArchive.CreateEntryFromFile(filePath, relativePath);
            }

            return Task.FromResult(Result<string>.Ok(Path.GetFileNameWithoutExtension(targetPath)));
        }

        public async Task<Result> RestoreSaveFileAsync(GameInfo gameInfo, string backupFileName)
        {
            logger.LogInformation("Start restore save file for Game : {GameName} UniqueId : {UniqueId}",
                gameInfo.GameName,
                gameInfo.GameUniqueId);
            string? saveFilePath = gameInfo.SaveFilePath;
            logger.LogInformation("SaveFilePath : {SaveFilePath}", saveFilePath);
            if (string.IsNullOrEmpty(saveFilePath))
            {
                logger.LogWarning("{GameInfoSaveFilePathName} : {MessageParameterNotSet}",
                    nameof(saveFilePath), Resources.Message_ParameterNotSet);
                return Result.Failure("SaveFilePath : " + Resources.Message_ParameterNotSet);
            }

            if (!Directory.Exists(saveFilePath))
            {
                logger.LogWarning("{GameInfoSaveFilePathName} : {MessageDirectoryNotExist}",
                    nameof(saveFilePath), Resources.Message_DirectoryNotExist);
                return Result.Failure("SaveFilePath : " + Resources.Message_DirectoryNotExist);
            }

            string filePath = Path.Combine(appPathService.SaveFileBackupDirPath, gameInfo.GameUniqueId.ToString(),
                backupFileName + ".zip");
            if (!File.Exists(filePath))
            {
                logger.LogWarning("Backup file not exist : {FilePath}", filePath);
                return Result.Failure("Backup file not exist");
            }

            ExtractorFactory extractorFactory = App.ServiceProvider.GetRequiredService<ExtractorFactory>();
            IExtractor zipExtractor = extractorFactory.GetExtractor(".zip") ??
                                      throw new ArgumentException(".zip extractor not found");
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            logger.LogInformation("Try extract file to temp path : {TempPath}", tempPath);
            Result<string> extractResult = await zipExtractor.ExtractAsync(filePath,
                new ExtractOption
                {
                    TargetPath = tempPath
                });
            if (!extractResult.Success)
            {
                logger.LogError("extract file to temp path failed : {ErrorMessage}", extractResult.Message);
                try
                {
                    Directory.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "error to delete temp file : {TempFilePath}", tempPath);
                }

                return Result.Failure("extract file to temp path failed : " + extractResult.Message);
            }

            try
            {
                logger.LogInformation("Copy file from temp path to save file path");
                FileHelper.CopyDirectory(tempPath, saveFilePath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error to copy file to target path");
                return Result.Failure("Error to copy file to target path", e);
            }

            return Result.Ok();
        }

        public Task<Result> DeleteSaveFileAsync(GameInfo gameInfo, string backupFileName)
        {
            logger.LogInformation("Start delete save file for Game : {GameName} UniqueId : {UniqueId}",
                gameInfo.GameName,
                gameInfo.GameUniqueId);
            string? saveFilePath = gameInfo.SaveFilePath;
            logger.LogInformation("SaveFilePath : {SaveFilePath}", saveFilePath);
            if (string.IsNullOrEmpty(saveFilePath))
            {
                logger.LogWarning("{GameInfoSaveFilePathName} : {MessageParameterNotSet}",
                    nameof(saveFilePath), Resources.Message_ParameterNotSet);
                return Task.FromResult(Result.Failure("SaveFilePath : " + Resources.Message_ParameterNotSet));
            }

            if (!Directory.Exists(saveFilePath))
            {
                logger.LogWarning("{GameInfoSaveFilePathName} : {MessageDirectoryNotExist}",
                    nameof(saveFilePath), Resources.Message_DirectoryNotExist);
                return Task.FromResult(Result.Failure("SaveFilePath : " + Resources.Message_DirectoryNotExist));
            }

            string filePath = Path.Combine(appPathService.SaveFileBackupDirPath, gameInfo.GameUniqueId.ToString(),
                backupFileName + ".zip");
            if (!File.Exists(filePath))
            {
                logger.LogWarning("Backup file not exist : {FilePath}", filePath);
                return Task.FromResult(Result.Failure("Backup file not exist"));
            }

            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error to delete file : {FilePath}", filePath);
                return Task.FromResult(Result.Failure("Error to delete file", e));
            }

            if (Directory.EnumerateFiles(saveFilePath).Any())
                return Task.FromResult(Result.Ok());
            try
            {
                Directory.Delete(saveFilePath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error to delete directory : {DirectoryPath}", Path.GetDirectoryName(filePath));
            }

            return Task.FromResult(Result.Ok());
        }
    }
}