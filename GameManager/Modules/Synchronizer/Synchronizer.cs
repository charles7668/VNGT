using GameManager.DTOs;
using GameManager.Modules.SaveDataManager;
using GameManager.Modules.SecurityProvider;
using GameManager.Modules.Synchronizer.Drivers;
using GameManager.Services;
using Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Web;
using CancellationToken = System.Threading.CancellationToken;
using FileInfo = GameManager.Models.FileInfo;
using InvalidOperationException = System.InvalidOperationException;

namespace GameManager.Modules.Synchronizer
{
    public class Synchronizer(
        IWebDAVDriver webDAVDriver,
        IConfigService configService,
        ISaveDataManager saveDataManager,
        ILogger<Synchronizer> logger,
        IAppPathService appPathService,
        ISecurityProvider securityProvider)
        : ISynchronizer
    {
        private const string BASE_REMOTE_PATH = "vngt";
        private const string TIME_FILE_NAME = "time.txt";

        public async Task SyncAppSetting(CancellationToken cancellationToken)
        {
            ResetConfig();
            logger.LogInformation("Syncing app setting");
            await webDAVDriver.CreateFolderIfNotExistsAsync(BuildPath(), cancellationToken);

            AppSettingDTO appSettingDto = configService.GetAppSettingDTO();
            string bodyString = JsonSerializer.Serialize(appSettingDto);
            DateTime syncTime = await CompareAndSync(BuildPath("appSetting.json"), BuildPath(TIME_FILE_NAME),
                bodyString,
                remoteTime => CompareTime(appSettingDto.UpdatedTime, remoteTime), downloadContent =>
                {
                    AppSettingDTO? tempDownloadDto =
                        JsonSerializer.Deserialize<AppSettingDTO>(Encoding.UTF8.GetString(downloadContent));
                    if (tempDownloadDto == null)
                        throw new InvalidOperationException("download content can't be deserialized");
                    // restore not sync data
                    tempDownloadDto.Id = appSettingDto.Id;
                    tempDownloadDto.WebDAVUrl = appSettingDto.WebDAVUrl;
                    tempDownloadDto.WebDAVUser = appSettingDto.WebDAVUser;
                    tempDownloadDto.WebDAVPassword = appSettingDto.WebDAVPassword;
                    tempDownloadDto.EnableSync = appSettingDto.EnableSync;

                    appSettingDto = tempDownloadDto;
                }, cancellationToken).ConfigureAwait(false);
            if (syncTime != appSettingDto.UpdatedTime)
            {
                appSettingDto.UpdatedTime = syncTime;
                await configService.UpdateAppSettingAsync(appSettingDto);
            }
        }

        public async Task SyncGameInfos(CancellationToken cancellationToken)
        {
            ResetConfig();
            logger.LogInformation("Syncing game infos start");
            int errorCount = await RemoveDeletedGameInfoAsync(cancellationToken);
            errorCount += await SyncLocalToRemote(cancellationToken);
            errorCount += await SyncRemoteToLocal(cancellationToken);

            if (errorCount > 0)
                throw new InvalidOperationException(
                    $"Failed to sync some game infos : {errorCount} items sync failed");
        }

        private static string BuildPath(params string[] paths)
        {
            var pathList = new List<string>
            {
                BASE_REMOTE_PATH
            };
            pathList.AddRange(paths);
            string path = Path.Combine(pathList.ToArray());
            return path.Replace("\\", "/");
        }

        private async Task SyncGameInfo(GameInfoDTO dto, CancellationToken cancellationToken)
        {
            GameInfoDTO resultDto = dto;
            await webDAVDriver.CreateFolderIfNotExistsAsync(BuildPath(), cancellationToken);
            await webDAVDriver.CreateFolderIfNotExistsAsync(BuildPath(dto.GameUniqueId),
                cancellationToken);
            string bodyString = JsonSerializer.Serialize(dto);
            DateTime lastedTime = await CompareAndSync(BuildPath(dto.GameUniqueId, "gameInfo.json"),
                BuildPath(dto.GameUniqueId, TIME_FILE_NAME), bodyString,
                remoteTime => CompareTime(dto.UpdatedTime, remoteTime), downloadContent =>
                {
                    GameInfoDTO? tempDownloadDto =
                        JsonSerializer.Deserialize<GameInfoDTO>(
                            Encoding.UTF8.GetString(downloadContent));
                    if (tempDownloadDto == null)
                        throw new InvalidOperationException(
                            "download content can't be deserialized");
                    // restore not sync data
                    tempDownloadDto.Id = dto.Id;
                    tempDownloadDto.GameUniqueId = dto.GameUniqueId;
                    resultDto = tempDownloadDto;
                }, cancellationToken).ConfigureAwait(false);
            if (CompareTime(resultDto.UpdatedTime, lastedTime) == TimeComparison.SAME)
            {
                return;
            }

            resultDto.UpdatedTime = lastedTime;
            await configService.UpdateGameInfoAsync(resultDto);
            List<string> remoteSaveFiles = [];
            await ExceptionHelper.ExecuteWithExceptionHandlingAsync(
                async () =>
                {
                    List<FileInfo> remoteFiles =
                        await webDAVDriver.GetFilesAsync(BuildPath(dto.GameUniqueId, "save-files"));
                    remoteSaveFiles = remoteFiles.Select(x =>
                            HttpUtility.UrlDecode(x.FileName.Split('/')[^1]))
                        .ToList();
                }, ex =>
                {
                    if (ex is not FileNotFoundException)
                        throw ex;
                    return Task.CompletedTask;
                });
            var localSaveFilePath = (await saveDataManager.GetBackupListAsync(dto))
                .Select(x => x + ".zip").ToList();
            var localSaveFileNames =
                localSaveFilePath.Select(x => x.Split('/')[^1]).ToList();
            var newestSaveFiles = remoteSaveFiles.Select(x => x).ToList();
            newestSaveFiles.AddRange(localSaveFilePath);
            newestSaveFiles.Sort(Comparer<string>.Create((a, b) =>
            {
                a = a.Replace(".zip", "");
                b = b.Replace(".zip", "");
                DateTime.TryParseExact(a, "yyyy-MM-dd HH-mm-ss-fff", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime dateTimeA);
                DateTime.TryParseExact(b, "yyyy-MM-dd HH-mm-ss-fff", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime dateTimeB);
                return dateTimeB.CompareTo(dateTimeA);
            }));
            newestSaveFiles = newestSaveFiles.Distinct().Take(saveDataManager.MaxBackupCount)
                .ToList();
            await webDAVDriver.CreateFolderIfNotExistsAsync($"/vngt/{dto.GameUniqueId}/save-files",
                cancellationToken);
            var remoteHashSet = remoteSaveFiles.ToHashSet();
            var localHashSet = localSaveFileNames.ToHashSet();
            var newFileHashSet = newestSaveFiles.ToHashSet();
            foreach (string newFile in newestSaveFiles)
            {
                if (!remoteHashSet.Contains(newFile))
                {
                    string filePath = Path.Combine(appPathService.SaveFileBackupDirPath,
                        dto.GameUniqueId, newFile);
                    await using var fileStream = new FileStream(filePath, FileMode.Open);
                    await webDAVDriver.UploadFileAsync(
                        $"/vngt/{dto.GameUniqueId}/save-files/{newFile}",
                        fileStream, cancellationToken);
                }
                else if (!localSaveFileNames.Contains(newFile))
                {
                    byte[] fileContent = await webDAVDriver.DownloadFileAsync(
                        $"/vngt/{dto.GameUniqueId}/save-files/{newFile}", cancellationToken);
                    string writePath = Path.Combine(appPathService.SaveFileBackupDirPath,
                        dto.GameUniqueId, newFile);
                    Directory.CreateDirectory(Path.GetDirectoryName(writePath) ?? "");
                    await using var fileStream = new FileStream(writePath, FileMode.Create);
                    await fileStream.WriteAsync(fileContent, cancellationToken);
                }
            }

            foreach (string remoteFile in remoteHashSet)
            {
                if (!newFileHashSet.Contains(remoteFile))
                {
                    await webDAVDriver.Delete(
                        BuildPath(dto.GameUniqueId, "save-files", remoteFile), cancellationToken);
                }
            }

            foreach (string localFile in localHashSet)
            {
                if (!newFileHashSet.Contains(localFile))
                {
                    ExceptionHelper.ExecuteWithExceptionHandling(() =>
                    {
                        File.Delete(Path.Combine(appPathService.SaveFileBackupDirPath,
                            dto.GameUniqueId, localFile));
                    });
                }
            }
        }

        private async Task<int> SyncLocalToRemote(CancellationToken cancellationToken)
        {
            logger.LogInformation("Syncing local to remote");
            List<int> ids = await configService.GetGameInfoIdCollectionAsync(x => x.EnableSync)
                .ConfigureAwait(false);
            int errorCount = 0;
            const int takeCountAtOnce = 30;
            while (ids.Count > 0)
            {
                var syncIds = ids.Take(takeCountAtOnce).ToList();
                ids = ids.Skip(takeCountAtOnce).ToList();
                // update exist game
                List<GameInfoDTO> dtos = await configService.GetGameInfoIncludeAllDTOsAsync(syncIds, 0,
                    takeCountAtOnce).ConfigureAwait(false);
                foreach (GameInfoDTO dto in dtos)
                {
                    try
                    {
                        await SyncGameInfo(dto, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to sync game {UniqueId}", dto.GameUniqueId);
                        errorCount++;
                    }
                }
            }

            return errorCount;
        }

        private async Task<int> SyncRemoteToLocal(CancellationToken cancellationToken)
        {
            logger.LogInformation("Syncing remote to local");
            int errorCount = 0;
            var dirs = (await webDAVDriver.GetDirectories(BuildPath(), 1))
                .Select(x => x.TrimEnd('/').Split('/')[^1]).ToHashSet();
            dirs.Remove(BASE_REMOTE_PATH);
            int countOfGameInfos = await configService.GetGameInfoCountAsync(_ => true);
            int takeCountAtOnce = 300;
            int takeTime = 0;
            while (countOfGameInfos > 0)
            {
                List<string> uniqueIds =
                    await configService.GetUniqueIdCollection(_ => true, takeTime * takeCountAtOnce,
                        takeCountAtOnce);
                countOfGameInfos -= takeCountAtOnce;
                foreach (string uniqueId in uniqueIds)
                {
                    dirs.Remove(uniqueId);
                }
            }

            foreach (string dir in dirs)
            {
                string uniqueId = dir;
                var dto = new GameInfoDTO
                {
                    GameUniqueId = uniqueId
                };
                try
                {
                    string bodyString = JsonSerializer.Serialize(dto);
                    DateTime lastedTime = await CompareAndSync(BuildPath(uniqueId, "gameInfo.json"),
                        BuildPath(uniqueId, TIME_FILE_NAME), bodyString,
                        remoteTime => CompareTime(dto.UpdatedTime, remoteTime), downloadContent =>
                        {
                            GameInfoDTO? tempDownloadDto =
                                JsonSerializer.Deserialize<GameInfoDTO>(
                                    Encoding.UTF8.GetString(downloadContent));
                            if (tempDownloadDto == null)
                                throw new InvalidOperationException("download content can't be deserialized");
                            // restore not sync data
                            tempDownloadDto.GameUniqueId = dto.GameUniqueId;
                            dto = tempDownloadDto;
                        }, cancellationToken).ConfigureAwait(false);
                    dto.UpdatedTime = lastedTime;
                    await configService.AddGameInfoAsync(dto, false);
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to sync game {UniqueId}", dto.GameUniqueId);
                    errorCount++;
                }
            }

            return errorCount;
        }

        private async Task<DateTime> GetRemoteUpdateTime(string filePath, CancellationToken cancellationToken)
        {
            DateTime remoteUpdateTime = DateTime.MinValue;
            try
            {
                FileInfo? timeFile =
                    (await webDAVDriver.GetFilesAsync(filePath)).FirstOrDefault();
                if (timeFile != null)
                {
                    byte[] timeContent =
                        await webDAVDriver.DownloadFileAsync(timeFile.FileName, cancellationToken);
                    string timeString = Encoding.UTF8.GetString(timeContent);
                    DateTime.TryParse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out remoteUpdateTime);
                }
            }
            catch (FileNotFoundException)
            {
                return DateTime.MinValue;
            }

            return remoteUpdateTime;
        }

        private async Task<int> RemoveDeletedGameInfoAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Removing deleted game infos from remote");
            List<PendingGameInfoDeletionDTO> pendingDeletions =
                await configService.GetPendingGameInfoDeletionUniqueIdsAsync();
            var deletedList = new List<PendingGameInfoDeletionDTO>();
            int errorCount = 0;
            foreach (PendingGameInfoDeletionDTO deletion in pendingDeletions)
            {
                string uniqueId = deletion.GameUniqueId;
                try
                {
                    DateTime remoteUpdateTime =
                        await GetRemoteUpdateTime(BuildPath(uniqueId, TIME_FILE_NAME), cancellationToken);
                    TimeComparison compareResult = CompareTime(deletion.DeletionDate, remoteUpdateTime);
                    if (compareResult == TimeComparison.REMOTE_IS_NEWER)
                    {
                        deletedList.Add(deletion);
                        continue;
                    }

                    await webDAVDriver.Delete(BuildPath(uniqueId), cancellationToken);
                    deletedList.Add(deletion);
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException)
                {
                    throw;
                }
                catch (FileNotFoundException)
                {
                    deletedList.Add(deletion);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to remove deleted game info {UniqueId}", deletion.GameUniqueId);
                    errorCount++;
                }
            }

            await ExceptionHelper.ExecuteWithExceptionHandlingAsync(async () =>
            {
                await configService.RemovePendingGameInfoDeletionsAsync(deletedList);
            }, ex =>
            {
                logger.LogError(ex, "Failed to remove deleted game info from database");
                return Task.CompletedTask;
            });

            return errorCount;
        }

        private static TimeComparison CompareTime(DateTime localTime, DateTime remoteTime)
        {
            // just compare to second
            var localNewTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour,
                localTime.Minute, localTime.Second, DateTimeKind.Utc);
            var remoteNewTime = new DateTime(remoteTime.Year, remoteTime.Month, remoteTime.Day, remoteTime.Hour,
                remoteTime.Minute, remoteTime.Second, DateTimeKind.Utc);

            if (remoteNewTime > localNewTime)
                return TimeComparison.REMOTE_IS_NEWER;
            return remoteNewTime < localNewTime ? TimeComparison.LOCAL_IS_NEWER : TimeComparison.SAME;
        }


        private void ResetConfig()
        {
            AppSettingDTO appSettingDTO = configService.GetAppSettingDTO();
            webDAVDriver.SetBaseUrl(appSettingDTO.WebDAVUrl);
            webDAVDriver.SetAuthentication(appSettingDTO.WebDAVUser,
                securityProvider.DecryptAsync(appSettingDTO.WebDAVPassword).Result);
        }

        private async Task<DateTime> CompareAndSync(string filePath, string timeFilePath, string compareString,
            Func<DateTime, TimeComparison> timeComparisonCallback,
            Action<byte[]> downloadedContentCallback,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Comparing and syncing {FilePath}", filePath);
            try
            {
                FileInfo? remoteFile = (await webDAVDriver.GetFilesAsync(filePath)).FirstOrDefault();
                if (remoteFile is null)
                    throw new FileNotFoundException();
                DateTime remoteTime = await GetRemoteUpdateTime(timeFilePath, cancellationToken);
                TimeComparison timeComparisonResult = timeComparisonCallback(remoteTime);
                switch (timeComparisonResult)
                {
                    case TimeComparison.LOCAL_IS_NEWER:
                        logger.LogInformation("Remote file is older than local one, updating");
                        await UpdateFile(filePath, compareString, cancellationToken);
                        DateTime time = DateTime.UtcNow;
                        await UpdateFile(timeFilePath, time.ToString(CultureInfo.InvariantCulture),
                            cancellationToken);
                        return time;
                    case TimeComparison.REMOTE_IS_NEWER:
                        {
                            logger.LogInformation("Local file is older than remote one, downloading");
                            byte[] remoteFileContent =
                                await webDAVDriver.DownloadFileAsync(filePath, cancellationToken);
                            downloadedContentCallback(remoteFileContent);
                            return remoteTime;
                        }
                    case TimeComparison.SAME:
                        logger.LogInformation("Local and remote file are the same, no need to sync");
                        return remoteTime;
                    default:
                        throw new InvalidDataException();
                }
            }
            catch (FileNotFoundException ex)
            {
                logger.LogInformation(ex, "Remote app setting not found, creating new one");
                DateTime time = DateTime.UtcNow;
                await UpdateFile(filePath, compareString, cancellationToken);
                await UpdateFile(timeFilePath, time.ToString(CultureInfo.InvariantCulture), cancellationToken);
                return time;
            }
            finally
            {
                logger.LogInformation("Syncing app setting completed");
            }

            async Task UpdateFile(string uploadFilePath, string content, CancellationToken token)
            {
                await webDAVDriver.UploadFileAsync(uploadFilePath,
                    new MemoryStream(Encoding.UTF8.GetBytes(content)), token);
                FileInfo? file = (await webDAVDriver.GetFilesAsync(uploadFilePath)).FirstOrDefault();
                if (file != null)
                    return;
                logger.LogError("Remote file not found after updating");
                throw new InvalidOperationException("Remote file not found after updating");
            }
        }

        private enum TimeComparison
        {
            LOCAL_IS_NEWER = 1,
            REMOTE_IS_NEWER = -1,
            SAME = 0
        }
    }
}