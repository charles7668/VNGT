using GameManager.Attributes;
using GameManager.DB.Models;

namespace GameManager.Services
{
    public abstract class BaseConfigService(string baseFolder) : IConfigService
    {
        [NeedCreate]
        public string ConfigFolder { get; } = baseFolder;

        public abstract void CreateConfigFolderIfNotExistAsync();

        public abstract Task<string> AddCoverImage(string srcFile);

        public abstract Task ReplaceCoverImage(string? srcFile, string? coverName);

        public abstract Task<string?> GetCoverFullPath(string? coverName);

        public abstract Task DeleteCoverImage(string? coverName);

        public abstract Task DeleteGameById(int id);

        public abstract Task AddGameInfo(GameInfo info);

        public abstract string GetDbPath();

        public abstract Task EditGameInfo(GameInfo info);
    }
}