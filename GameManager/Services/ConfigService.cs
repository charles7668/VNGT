using GameManager.Database;
using GameManager.DB.Models;
using GameManager.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;
using System.Text.Json;

namespace GameManager.Services
{
    /// <summary>
    /// the config service for debug
    /// </summary>
    public class ConfigService : IConfigService
    {
        public ConfigService(IServiceProvider serviceProvider)
        {
            _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider = serviceProvider;
            _appSetting = _unitOfWork.AppSettingRepository.GetAppSettingAsync().Result;
            _appPathService = serviceProvider.GetRequiredService<IAppPathService>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
        }

        private readonly IAppPathService _appPathService;

        private readonly AppSetting _appSetting;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly IServiceProvider _serviceProvider;

        private readonly IUnitOfWork _unitOfWork;

        private MemoryCache _memoryCache;

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
            string fullPath = Path.Combine(_appPathService.CoverDirPath, coverName);
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
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            try
            {
                IGameInfoRepository gameInfoRepo = unitOfWork.GameInfoRepository;
                string? cover = await gameInfoRepo.GetCoverById(id);
                if (cover != null)
                {
                    await DeleteCoverImage(cover);
                }

                GameInfo? deletedInfo = await unitOfWork.GameInfoRepository.DeleteByIdAsync(id);
                if (deletedInfo?.LaunchOption != null)
                    await unitOfWork.LaunchOptionRepository.Delete(deletedInfo.LaunchOption.Id);
            }
            finally
            {
                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteGameInfoByIdListAsync(IEnumerable<int> idList, CancellationToken cancellationToken,
            Action<int> onDeleteCallback)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await Parallel.ForEachAsync(idList, cancellationToken, async (id, token) =>
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await using AsyncServiceScope subScope = _serviceProvider.CreateAsyncScope();
                    IUnitOfWork subUnitOfWork = subScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    IGameInfoRepository gameInfoRepo = subUnitOfWork.GameInfoRepository;
                    string? cover = await gameInfoRepo.GetCoverById(id);
                    if (cover != null)
                    {
                        await DeleteCoverImage(cover);
                    }

                    await _semaphore.WaitAsync(token);
                    await unitOfWork.GameInfoRepository.DeleteByIdAsync(id);
                }
                finally
                {
                    if (_semaphore.CurrentCount == 0)
                        _semaphore.Release();
                    onDeleteCallback(id);
                }
            });
            await unitOfWork.SaveChangesAsync();
        }

        public async Task AddGameInfoAsync(GameInfo info)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            string[] tagArray = info.Tags.Select(x => x.Name).ToArray();
            info.Tags = [];
            do
            {
                info.GameUniqeId = Guid.NewGuid();
                if (await unitOfWork.GameInfoRepository.AnyAsync(x => x.GameUniqeId == info.GameUniqeId))
                    continue;
                break;
            } while (true);

            GameInfo gameInfoEntity = await unitOfWork.GameInfoRepository.AddAsync(info);
            await unitOfWork.SaveChangesAsync();
            info.Id = gameInfoEntity.Id;
            await UpdateGameInfoTags(info.Id, tagArray);
        }

        public Task<GameInfo?> GetGameInfoAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return _unitOfWork.GameInfoRepository.GetAsync(queryExpression);
        }

        public Task GetGameInfoForEachAsync(Action<GameInfo> action, CancellationToken cancellationToken,
            SortOrder order = SortOrder.UPLOAD_TIME)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return unitOfWork.GameInfoRepository
                .GetGameInfoForEachAsync(action, cancellationToken, order);
        }


        public async Task EditGameInfo(GameInfo info)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            List<Staff> staffs = info.Staffs;
            List<Character> characters = info.Characters;
            List<ReleaseInfo> releaseInfos = info.ReleaseInfos;
            List<RelatedSite> relatedSites = info.RelatedSites;
            info.Staffs = [];
            info.Characters = [];
            info.ReleaseInfos = [];
            info.RelatedSites = [];
            await unitOfWork.GameInfoRepository.EditAsync(info);
            await unitOfWork.GameInfoRepository.UpdateStaffsAsync(x => x.Id == info.Id, staffs);
            await unitOfWork.GameInfoRepository.UpdateCharactersAsync(x => x.Id == info.Id, characters);
            await unitOfWork.GameInfoRepository.UpdateReleaseInfosAsync(x => x.Id == info.Id, releaseInfos);
            await unitOfWork.GameInfoRepository.UpdateReltedSitesAsync(x => x.Id == info.Id, relatedSites);
            await unitOfWork.SaveChangesAsync();
        }

        public AppSetting GetAppSetting()
        {
            return _appSetting;
        }

        public async Task UpdateAppSettingAsync(AppSetting setting)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.AppSettingRepository.UpdateAppSettingAsync(setting);
            await unitOfWork.SaveChangesAsync();
            // clear cache because the data has been changed
            _memoryCache.Dispose();
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
        }

        public Task<List<Library>> GetLibrariesAsync(CancellationToken cancellationToken)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return unitOfWork.LibraryRepository.GetLibrariesAsync(cancellationToken);
        }

        public async Task AddLibraryAsync(Library library)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.LibraryRepository.AddAsync(library);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteLibraryByIdAsync(int id)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.LibraryRepository.DeleteByIdAsync(id);
            await unitOfWork.SaveChangesAsync();
        }

        public Task<bool> CheckExePathExist(string path)
        {
            return _unitOfWork.GameInfoRepository.CheckExePathExist(path);
        }

        public async Task UpdateLastPlayedByIdAsync(int id, DateTime time)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.GameInfoRepository.UpdateLastPlayedByIdAsync(id, time);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task BackupSettings(string path)
        {
            AppSetting setting = await _unitOfWork.AppSettingRepository.GetAppSettingAsync();
            await using FileStream fileStream = new(path, FileMode.Create);
            await JsonSerializer.SerializeAsync(fileStream, setting);
        }

        public async Task RestoreSettings(string path)
        {
            await using FileStream fileStream = new(path, FileMode.Open);
            AppSetting? setting = await JsonSerializer.DeserializeAsync<AppSetting>(fileStream);
            if (setting == null)
                return;
            await UpdateAppSettingAsync(setting);
        }

        public async Task<IEnumerable<StaffRole>> GetStaffRolesAsync()
        {
            IEnumerable<StaffRole> roles = await _unitOfWork.StaffRoleRepository.GetAsync(_ => true);
            return roles;
        }

        public Task<Staff?> GetStaffAsync(Expression<Func<Staff, bool>> query)
        {
            return _unitOfWork.StaffRepository.GetAsync(query);
        }

        public Task<IEnumerable<Staff>> GetGameInfoStaffs(Expression<Func<GameInfo, bool>> query)
        {
            return _unitOfWork.GameInfoRepository.GetStaffsAsync(query);
        }

        public Task<IEnumerable<Character>> GetGameInfoCharacters(Expression<Func<GameInfo, bool>> query)
        {
            return _unitOfWork.GameInfoRepository.GetCharactersAsync(query);
        }

        public Task<IEnumerable<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query)
        {
            return _unitOfWork.GameInfoRepository.GetGameInfoReleaseInfos(query);
        }

        public Task<IEnumerable<RelatedSite>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query)
        {
            return _unitOfWork.GameInfoRepository.GetGameInfoRelatedSites(query);
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
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var tagSet = tags.ToArray().ToHashSet();
            IEnumerable<Tag> originTags = await unitOfWork.GameInfoRepository.GetTagsByIdAsync(gameId);
            foreach (Tag tag in originTags)
            {
                if (tagSet.Contains(tag.Name))
                {
                    tagSet.Remove(tag.Name);
                    continue;
                }

                await unitOfWork.GameInfoTagRepository.RemoveGameInfoTagAsync(tag.Id, gameId);
            }

            foreach (string tag in tagSet)
            {
                Tag tagEntity = await unitOfWork.TagRepository.AddTagAsync(tag);
                await unitOfWork.GameInfoRepository.AddTagAsync(gameId, tagEntity);
            }

            await unitOfWork.SaveChangesAsync();
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