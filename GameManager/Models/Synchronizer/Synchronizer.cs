using GameManager.DTOs;
using GameManager.Models.Synchronizer.Drivers;
using GameManager.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CancellationToken = System.Threading.CancellationToken;
using InvalidOperationException = System.InvalidOperationException;

namespace GameManager.Models.Synchronizer
{
    public class Synchronizer(IWebDAVDriver webDAVDriver, IConfigService configService, ILogger<Synchronizer> logger)
        : ISynchronizer
    {
        public async Task SyncAppSetting(CancellationToken cancellationToken)
        {
            ResetConfig();
            logger.LogInformation("Syncing app setting");
            try
            {
                await webDAVDriver.CreateFolderIfNotExistsAsync("vngt", cancellationToken);

                AppSettingDTO appSettingDto = configService.GetAppSettingDTO();
                string bodyString = JsonSerializer.Serialize(appSettingDto);
                DateTime syncTime = await CompareAndSync("/vngt/appSetting.json", "/vngt/time.txt", bodyString,
                    remoteTime =>
                    {
                        if (remoteTime > appSettingDto.UpdatedTime)
                            return TimeComparison.REMOTE_IS_NEWER;
                        return remoteTime < appSettingDto.UpdatedTime
                            ? TimeComparison.LOCAL_IS_NEWER
                            : TimeComparison.SAME;
                    }, downloadContent =>
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
            catch (TaskCanceledException)
            {
                logger.LogInformation("Sync app setting canceled");
            }
        }

        public Task SyncGameInfos(CancellationToken cancellationToken)
        {
            try
            {
                Task.Run(async () =>
                {
                    ResetConfig();
                    logger.LogInformation("Syncing game infos start");
                    List<int> ids = await configService.GetGameInfoIdCollectionAsync(_ => true).ConfigureAwait(false);
                    int errorCount = 0;
                    int takeCountAtOnce = 30;
                    while (ids.Count > 0)
                    {
                        var syncIds = ids.Take(takeCountAtOnce).ToList();
                        ids = ids.Skip(takeCountAtOnce).ToList();
                        // update exist game
                        List<GameInfoDTO> dtos = await configService.GetGameInfoDTOsAsync(syncIds, 0, takeCountAtOnce);
                        foreach (GameInfoDTO dto in dtos)
                        {
                            try
                            {
                                GameInfoDTO resultDto = dto;
                                await webDAVDriver.CreateFolderIfNotExistsAsync("vngt", cancellationToken);
                                await webDAVDriver.CreateFolderIfNotExistsAsync($"vngt/{dto.GameUniqueId}",
                                    cancellationToken);
                                string bodyString = JsonSerializer.Serialize(dto);
                                DateTime lastedTime = await CompareAndSync($"/vngt/{dto.GameUniqueId}/gameInfo.json",
                                    $"/vngt/{dto.GameUniqueId}/time.txt", bodyString,
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
                                resultDto.UpdatedTime = lastedTime;
                                await configService.UpdateGameInfoAsync(resultDto);
                            }
                            catch (TaskCanceledException)
                            {
                                logger.LogInformation("Sync game infos canceled");
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "Failed to sync game {UniqueId}", dto.GameUniqueId);
                                errorCount++;
                            }
                        }
                    }

                    var dirs = (await webDAVDriver.GetDirectories("vngt", 1))
                        .Select(x => x.TrimEnd('/').Split('/').Last()).ToHashSet();
                    int countOfGameInfos = await configService.GetGameInfoCountAsync(_ => true);
                    takeCountAtOnce = 300;
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
                        if (dir == "vngt")
                            continue;
                        string uniqueId = dir.TrimEnd('/').Split('/').Last();
                        var dto = new GameInfoDTO
                        {
                            GameUniqueId = uniqueId
                        };
                        try
                        {
                            string bodyString = JsonSerializer.Serialize(dto);
                            DateTime lastedTime = await CompareAndSync($"/vngt/{uniqueId}/gameInfo.json",
                                $"/vngt/{uniqueId}/time.txt", bodyString,
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
                            logger.LogInformation("Sync game infos canceled");
                            throw;
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Failed to sync game {UniqueId}", dto.GameUniqueId);
                            errorCount++;
                        }
                    }

                    if (errorCount > 0)
                        throw new InvalidOperationException(
                            $"Failed to sync some game infos : {errorCount} items sync failed");
                }, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Sync game infos canceled");
            }

            return Task.CompletedTask;
        }

        private static TimeComparison CompareTime(DateTime localTime, DateTime remoteTime)
        {
            // just compare to second
            var localNewTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour,
                localTime.Minute, localTime.Second);
            var remoteNewTime = new DateTime(remoteTime.Year, remoteTime.Month, remoteTime.Day, remoteTime.Hour,
                remoteTime.Minute, remoteTime.Second);

            if (remoteNewTime > localNewTime)
                return TimeComparison.REMOTE_IS_NEWER;
            return remoteNewTime < localNewTime ? TimeComparison.LOCAL_IS_NEWER : TimeComparison.SAME;
        }


        private void ResetConfig()
        {
            AppSettingDTO appSettingDTO = configService.GetAppSettingDTO();
            webDAVDriver.SetBaseUrl(appSettingDTO.WebDAVUrl);
            webDAVDriver.SetAuthentication(appSettingDTO.WebDAVUser, appSettingDTO.WebDAVPassword);
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
                FileInfo? timeFile = (await webDAVDriver.GetFilesAsync(timeFilePath)).FirstOrDefault();
                byte[]? timeContent = timeFile != null
                    ? await webDAVDriver.DownloadFileAsync(timeFilePath, cancellationToken)
                    : null;
                string? timeString = timeContent != null
                    ? Encoding.UTF8.GetString(timeContent)
                    : null;
                if (!DateTime.TryParse(timeString, out DateTime remoteTime))
                    remoteTime = DateTime.MinValue;
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
                        return remoteFile.ModifiedTime;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (FileNotFoundException)
            {
                logger.LogInformation("Remote app setting not found, creating new one");
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