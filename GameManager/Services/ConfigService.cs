using GameManager.Attributes;
using GameManager.Database;
using GameManager.DB.Models;
using System.Reflection;

namespace GameManager.Services
{
    /// <summary>
    /// the config service for debug
    /// </summary>
    public class ConfigService : IConfigService
    {
        public ConfigService()
        {
            CreateConfigFolderIfNotExistAsync();
        }

        public ConfigService(IUnitOfWork unitOfWork) : this()
        {
            _unitOfWork = unitOfWork;
        }

        private const string DB_FILE = "game.db";

        private readonly IUnitOfWork _unitOfWork = null!;

        [NeedCreate]
        private string CoverFolder => Path.Combine(ConfigFolder, "covers");

        [NeedCreate]
        public string ConfigFolder { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs");

        public void CreateConfigFolderIfNotExistAsync()
        {
            PropertyInfo[] properties =
                GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttribute<NeedCreateAttribute>() == null)
                    continue;
                object? obj = propertyInfo.GetValue(this);
                if (obj is not string dir)
                {
                    continue;
                }

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        public async Task<string> AddCoverImage(string srcFile)
        {
            // generate random file name
            string id = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(srcFile);
            await ReplaceCoverImage(srcFile, id + extension);
            return id + extension;
        }

        public async Task ReplaceCoverImage(string? srcFile, string? coverName)
        {
            if (coverName == null || srcFile == null)
                throw new ArgumentException("path is null");
            if (!File.Exists(srcFile))
                throw new ArgumentException("file is not exist");
            string? coverPath = await GetCoverFullPath(coverName);
            if (coverPath == null)
                return;
            if (srcFile == coverPath)
                return;
            File.Copy(srcFile, coverPath, true);
        }

        public Task<string?> GetCoverFullPath(string? coverName)
        {
            if (coverName == null)
                return Task.FromResult<string?>(null);
            string fullPath = Path.Combine(CoverFolder, coverName);
            return Task.FromResult(fullPath)!;
        }

        public async Task DeleteCoverImage(string? coverName)
        {
            if (coverName == null)
                return;
            string? fullPath = await GetCoverFullPath(coverName);
            if (fullPath == null || !File.Exists(fullPath))
                return;
            File.Delete(fullPath);
        }

        public async Task DeleteGameById(int id)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            string? cover = await gameInfoRepo.GetCoverById(id);
            if (cover == null)
                return;
            await DeleteCoverImage(cover);
            await gameInfoRepo.DeleteByIdAsync(id);
        }

        public async Task AddGameInfo(GameInfo info)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            await gameInfoRepo.AddAsync(info);
        }

        public string GetDbPath()
        {
            return Path.Combine(ConfigFolder, DB_FILE);
        }

        public Task EditGameInfo(GameInfo info)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            return gameInfoRepo.EditAsync(info);
        }
    }
}