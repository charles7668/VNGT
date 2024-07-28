using GameManager.Attributes;
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
            _serviceProvider = serviceProvider;
            _appSetting = _unitOfWork.AppSettingRepository.GetAppSettingAsync().Result;
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
        }

        private const string DB_FILE = "game.db";

        private readonly IUnitOfWork _unitOfWork = null!;

        private readonly AppSetting _appSetting = null!;

        [NeedCreate]
        private string CoverFolder => Path.Combine(ConfigFolder, "covers");

        [NeedCreate]
#if DEBUG
        public string ConfigFolder { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs");
#else
        public string ConfigFolder { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VNGT", "configs");
#endif

        private readonly IServiceProvider _serviceProvider = null!;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private MemoryCache _memoryCache = null!;

        public void CreateConfigFolderIfNotExistAsync()
        {
            PropertyInfo[] properties =
                GetType()
                    .GetProperties(BindingFlags.Public
                                   | BindingFlags.NonPublic
                                   | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttribute<NeedCreateAttribute>() == null)
                    continue;
                object? obj = propertyInfo.GetValue(this);
                if (obj is not string dir)
                {
                    continue;
                }

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

        public async Task DeleteGameInfoByIdAsync(int id)
        {
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IGameInfoRepository gameInfoRepo = unitOfWork.GameInfoRepository;
                string? cover = await gameInfoRepo.GetCoverById(id);
                if (cover != null)
                {
                    await DeleteCoverImage(cover);
                }

                await _semaphore.WaitAsync();
                await _unitOfWork.GameInfoRepository.DeleteByIdAsync(id);
            }
            finally
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.ClearChangeTrackerAsync();
                if (_semaphore.CurrentCount == 0)
                    _semaphore.Release();
            }
        }

        public async Task DeleteGameInfoByIdListAsync(IEnumerable<int> idList, CancellationToken cancellationToken,
            Action<int> onDeleteCallback)
        {
            await Parallel.ForEachAsync(idList, cancellationToken, async (id, token) =>
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                    IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    IGameInfoRepository gameInfoRepo = unitOfWork.GameInfoRepository;
                    string? cover = await gameInfoRepo.GetCoverById(id);
                    if (cover != null)
                    {
                        await DeleteCoverImage(cover);
                    }

                    await _semaphore.WaitAsync(token);
                    await _unitOfWork.GameInfoRepository.DeleteByIdAsync(id);
                }
                finally
                {
                    if (_semaphore.CurrentCount == 0)
                        _semaphore.Release();
                    onDeleteCallback(id);
                }
            });
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearChangeTrackerAsync();
        }

        public async Task AddGameInfoAsync(GameInfo info)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            await gameInfoRepo.AddAsync(info);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearChangeTrackerAsync();
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

        public string GetToolPath()
        {
            return Path.Combine(ConfigFolder, "tools");
        }

        public async Task EditGameInfo(GameInfo info)
        {
            IGameInfoRepository gameInfoRepo = _unitOfWork.GameInfoRepository;
            await gameInfoRepo.EditAsync(info);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearChangeTrackerAsync();
        }

        public AppSetting GetAppSetting()
        {
            return _appSetting;
        }

        public async Task UpdateAppSettingAsync(AppSetting setting)
        {
            await _unitOfWork.AppSettingRepository.UpdateAppSettingAsync(setting);
            await _unitOfWork.SaveChangesAsync();
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

        public async Task AddLibraryAsync(Library library)
        {
            await _unitOfWork.LibraryRepository.AddAsync(library);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteLibraryByIdAsync(int id)
        {
            await _unitOfWork.LibraryRepository.DeleteByIdAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<bool> CheckExePathExist(string path)
        {
            return _unitOfWork.GameInfoRepository.CheckExePathExist(path);
        }

        public async Task UpdateLastPlayedByIdAsync(int id, DateTime time)
        {
            await _unitOfWork.GameInfoRepository.UpdateLastPlayedByIdAsync(id, time);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearChangeTrackerAsync();
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

        public async Task<IEnumerable<string>> GetGameTagsAsync(int gameId)
        {
            IEnumerable<Tag> tags = await _unitOfWork.GameInfoRepository
                .GetTagsByIdAsync(gameId);
            IEnumerable<string> result = tags.Select(x => x.Name);
            return result;
        }

        public async Task UpdateGameInfoTags(int gameId, IEnumerable<string> tags)
        {
            var tagSet = tags.ToArray().ToHashSet();
            IEnumerable<Tag> originTags = await _unitOfWork.GameInfoRepository.GetTagsByIdAsync(gameId);
            foreach (Tag tag in originTags)
            {
                if (tagSet.Contains(tag.Name))
                {
                    tagSet.Remove(tag.Name);
                    continue;
                }

                await _unitOfWork.GameInfoTagRepository.RemoveGameInfoTagAsync(tag.Id, gameId);
            }

            foreach (string tag in tagSet)
            {
                Tag tagEntity = await _unitOfWork.TagRepository.AddTagAsync(tag);
                await _unitOfWork.GameInfoRepository.AddTagAsync(gameId, tagEntity);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearChangeTrackerAsync();
        }

        public async Task<bool> CheckGameInfoHasTag(int gameId, string tagName)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            Tag? tag = await unitOfWork.TagRepository.AnyAsync(tagName);
            if (tag == null)
            {
                return false;
            }

            bool hasTag = await unitOfWork.GameInfoTagRepository.CheckGameHasTag(tag.Id, gameId);
            return hasTag;
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            return await _unitOfWork.TagRepository.GetAllTagsAsync();
        }
    }
}