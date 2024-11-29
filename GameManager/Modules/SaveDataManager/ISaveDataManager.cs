using GameManager.DTOs;
using GameManager.Models;

namespace GameManager.Modules.SaveDataManager
{
    public interface ISaveDataManager
    {
        public int MaxBackupCount { get; }

        public Task<List<string>> GetBackupListAsync(GameInfoDTO gameInfo);

        /// <summary>
        /// backup save file , return backup file name
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public Task<Result<string>> BackupSaveFileAsync(GameInfoDTO gameInfo);

        /// <summary>
        /// Restore save file using specified backup file
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="backupFileName"></param>
        /// <returns></returns>
        public Task<Result> RestoreSaveFileAsync(GameInfoDTO gameInfo, string backupFileName);

        /// <summary>
        /// Delete save file using specified backup file
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="backupFileName"></param>
        /// <returns></returns>
        public Task<Result> DeleteSaveFileAsync(GameInfoDTO gameInfo, string backupFileName);
    }
}