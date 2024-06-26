﻿using GameManager.Attributes;
using GameManager.Database;
using GameManager.DB.Models;
using GameManager.Enums;
using Microsoft.Extensions.Caching.Memory;
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

        public ConfigService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : this()
        {
            _unitOfWork = unitOfWork;
            ServiceProvider = serviceProvider;
            AppSetting = _unitOfWork.AppSettingRepository.GetAppSettingAsync().Result;
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
        }

        private const string DB_FILE = "game.db";

        private readonly IUnitOfWork _unitOfWork = null!;

        private AppSetting AppSetting { get; } = null!;

        [NeedCreate]
        private string CoverFolder => Path.Combine(ConfigFolder, "covers");

        [NeedCreate]
#if DEBUG
        public string ConfigFolder { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs");
#else
        public string ConfigFolder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VNGT", "configs");
#endif

        private IServiceProvider ServiceProvider { get; } = null!;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private IMemoryCache _memoryCache = null!;

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
            try
            {
                await using AsyncServiceScope scope = ServiceProvider.CreateAsyncScope();
                IGameInfoRepository gameInfoRepo = scope.ServiceProvider
                    .GetRequiredService<IGameInfoRepository>();
                string? cover = await gameInfoRepo.GetCoverById(id);
                if (cover != null)
                {
                    await DeleteCoverImage(cover);
                }

                await _semaphore.WaitAsync();
                await gameInfoRepo.DeleteByIdAsync(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddGameInfoAsync(GameInfo info)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            await gameInfoRepo.AddAsync(info);
        }

        public Task GetGameInfoForEachAsync(Action<GameInfo> action, CancellationToken cancellationToken,
            SortOrder order = SortOrder.UPLOAD_TIME)
        {
            return _unitOfWork.GameInfoRepository
                .GetGameInfoForEachAsync(action, cancellationToken, order);
        }

        public string GetDbPath()
        {
            return Path.Combine(ConfigFolder, DB_FILE);
        }

        public string GetLogPath()
        {
            return Path.Combine(ConfigFolder, "logs");
        }

        public Task EditGameInfo(GameInfo info)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            return gameInfoRepo.EditAsync(info);
        }

        public AppSetting GetAppSetting()
        {
            return AppSetting;
        }

        public async Task UpdateAppSettingAsync(AppSetting setting)
        {
            await _unitOfWork.AppSettingRepository.UpdateAppSettingAsync(setting);
            // clear cache because the data has been changed
            _memoryCache.Dispose();
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
        }

        public Task<List<Library>> GetLibrariesAsync(CancellationToken cancellationToken)
        {
            return _unitOfWork.LibraryRepository.GetLibrariesAsync(cancellationToken);
        }

        public Task AddLibraryAsync(Library library)
        {
            return _unitOfWork.LibraryRepository.AddAsync(library);
        }

        public Task DeleteLibraryByIdAsync(int id)
        {
            return _unitOfWork.LibraryRepository.DeleteByIdAsync(id);
        }

        public Task<bool> CheckExePathExist(string path)
        {
            return _unitOfWork.GameInfoRepository.CheckExePathExist(path);
        }

        public Task UpdateLastPlayedByIdAsync(int id, DateTime time)
        {
            return _unitOfWork.GameInfoRepository.UpdateLastPlayedByIdAsync(id, time);
        }

        public async Task<TextMapping?> SearchTextMappingByOriginalText(string original)
        {
            string key = "TextMapping::" + original;
            if (_memoryCache.TryGetValue(key, out TextMapping? mapping))
            {
                return mapping;
            }

            mapping = await _unitOfWork.AppSettingRepository.SearchTextMappingByOriginalText(original);
            _memoryCache.Set(key, mapping, new MemoryCacheEntryOptions
            {
                Size = 1
            });
            return mapping;
        }
    }
}