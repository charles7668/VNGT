using GameManager.DTOs;
using GameManager.Models.Synchronizer.Drivers;
using GameManager.Services;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using CancellationToken = System.Threading.CancellationToken;

namespace GameManager.Models.Synchronizer
{
    public class Synchronizer(IWebDAVDriver webDAVDriver, IConfigService configService, ILogger<Synchronizer> logger)
        : ISynchronizer
    {
        public async Task SyncAppSetting(CancellationToken cancellationToken)
        {
            ResetConfig();
            logger.LogInformation("Syncing app setting");
            await webDAVDriver.CreateFolderIfNotExistsAsync("vngt", cancellationToken);

            AppSettingDTO appSettingDto = configService.GetAppSettingDTO();
            string bodyString = JsonSerializer.Serialize(appSettingDto);
            DateTime syncTime = await CompareAndSync("/vngt/appSetting.json", bodyString, remoteTime =>
            {
                if (remoteTime > appSettingDto.UpdatedTime)
                    return TimeComparison.REMOTE_IS_NEWER;
                return remoteTime < appSettingDto.UpdatedTime ? TimeComparison.LOCAL_IS_NEWER : TimeComparison.SAME;
            }, downloadContent =>
            {
                AppSettingDTO? tempDownloadDto =
                    JsonSerializer.Deserialize<AppSettingDTO>(Encoding.UTF8.GetString(downloadContent));
                if (tempDownloadDto == null)
                    throw new Exception("download content can't be deserialized");
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

        private void ResetConfig()
        {
            AppSettingDTO appSettingDTO = configService.GetAppSettingDTO();
            webDAVDriver.SetBaseUrl(appSettingDTO.WebDAVUrl);
            webDAVDriver.SetAuthentication(appSettingDTO.WebDAVUser, appSettingDTO.WebDAVPassword);
        }

        private async Task<DateTime> CompareAndSync(string filePath, string compareString,
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
                if (!remoteFile.FileName.EndsWith(filePath))
                    throw new FileNotFoundException();
                TimeComparison timeComparisonResult = timeComparisonCallback(remoteFile.ModifiedTime);
                switch (timeComparisonResult)
                {
                    case TimeComparison.LOCAL_IS_NEWER:
                        logger.LogInformation("Remote file is older than local one, updating");
                        return await UpdateFile(filePath, compareString, cancellationToken);
                    case TimeComparison.REMOTE_IS_NEWER:
                        {
                            logger.LogInformation("Local file is older than remote one, downloading");
                            byte[] remoteFileContent =
                                await webDAVDriver.DownloadFileAsync(filePath, cancellationToken);
                            downloadedContentCallback(remoteFileContent);
                            return remoteFile.ModifiedTime;
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
                return await UpdateFile(filePath, compareString, cancellationToken);
            }
            finally
            {
                logger.LogInformation("Syncing app setting completed");
            }

            async Task<DateTime> UpdateFile(string uploadFilePath, string content, CancellationToken token)
            {
                await webDAVDriver.UploadFileAsync(uploadFilePath,
                    new MemoryStream(Encoding.UTF8.GetBytes(content)), token);
                FileInfo? file = (await webDAVDriver.GetFilesAsync(uploadFilePath)).FirstOrDefault();
                if (file != null && !file.FileName.EndsWith(uploadFilePath)) return file.ModifiedTime;
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