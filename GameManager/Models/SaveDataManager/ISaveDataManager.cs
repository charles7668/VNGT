using GameManager.DB.Models;

namespace GameManager.Models.SaveDataManager
{
    public interface ISaveDataManager
    {
        public int MaxBackupCount { get; }

        public Task<List<string>> GetBackupListAsync(GameInfo gameInfo);

        /// <summary>
        /// backup save file , return backup file name
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public Task<Result<string>> BackupSaveFileAsync(GameInfo gameInfo);

        /// <summary>
        /// Restore save file using specified backup file
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="backupFileName"></param>
        /// <returns></returns>
        public Task<Result> RestoreSaveFileAsync(GameInfo gameInfo, string backupFileName);

        /// <summary>
        /// Delete save file using specified backup file
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="backupFileName"></param>
        /// <returns></returns>
        public Task<Result> DeleteSaveFileAsync(GameInfo gameInfo, string backupFileName);
    }
}