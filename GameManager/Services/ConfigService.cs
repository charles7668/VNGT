using GameManager.Database;
using GameManager.DB;
using GameManager.DB.Models;
using GameManager.DTOs;
using GameManager.Enums;
using Microsoft.EntityFrameworkCore;
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

        private AppSetting _appSetting;

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

        public AppSettingDTO GetAppSettingDTO()
        {
            return AppSettingDTO.Create(_appSetting);
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

        public async Task AddGameInfoAsync(GameInfo info, bool generateUniqueId = true)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            unitOfWork.BeginTransaction();
            if (generateUniqueId)
            {
                do
                {
                    info.GameUniqueId = Guid.NewGuid().ToString();
                    if (await unitOfWork.GameInfoRepository.AnyAsync(x => x.GameUniqueId == info.GameUniqueId))
                        continue;
                    break;
                } while (true);
            }

            // set default background image
            if (string.IsNullOrEmpty(info.BackgroundImageUrl) && info.ScreenShots.Count > 0)
            {
                info.BackgroundImageUrl = info.ScreenShots[0];
            }

            List<Tag> tags = info.Tags;
            List<Staff> staffs = info.Staffs;
            List<Character> characters = info.Characters;
            List<ReleaseInfo> releaseInfos = info.ReleaseInfos;
            List<RelatedSite> relatedSites = info.RelatedSites;
            info.Tags = [];
            info.Staffs = [];
            info.Characters = [];
            info.ReleaseInfos = [];
            info.RelatedSites = [];

            try
            {
                info.UploadTime = DateTime.UtcNow;
                info.UpdatedTime = DateTime.UtcNow;
                GameInfo gameInfoEntity = await unitOfWork.GameInfoRepository.AddAsync(info);
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.GameInfoRepository.UpdateStaffsAsync(x => x.Id == gameInfoEntity.Id, staffs);
                await unitOfWork.GameInfoRepository.UpdateCharactersAsync(x => x.Id == gameInfoEntity.Id, characters);
                await unitOfWork.GameInfoRepository.UpdateReleaseInfosAsync(x => x.Id == gameInfoEntity.Id,
                    releaseInfos);
                await unitOfWork.GameInfoRepository.UpdateRelatedSitesAsync(x => x.Id == gameInfoEntity.Id,
                    relatedSites);
                await unitOfWork.GameInfoRepository.UpdateTagsAsync(x => x.Id == gameInfoEntity.Id, tags);
                await unitOfWork.SaveChangesAsync();
                unitOfWork.CommitTransaction();
                info.Id = gameInfoEntity.Id;
                info.Tags = tags;
                info.Staffs = staffs;
                info.Characters = characters;
                info.ReleaseInfos = releaseInfos;
                info.RelatedSites = relatedSites;
            }
            catch (Exception)
            {
                unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public Task AddGameInfoAsync(GameInfoDTO dto , bool generateUniqueId = true)
        {
            GameInfo gameInfo = dto.Convert();
            gameInfo.Id = 0;
            gameInfo.LaunchOptionId = 0;
            gameInfo.LaunchOption ??= new LaunchOption();
            gameInfo.LaunchOption.Id = 0;
            return AddGameInfoAsync(gameInfo, generateUniqueId);
        }

        public async Task<GameInfo?> GetGameInfoAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            GameInfo? entity = await _unitOfWork.GameInfoRepository.GetAsync(queryExpression);
            if (entity == null)
                return null;
            _unitOfWork.DetachEntity(entity);
            return entity;
        }

        public async Task<GameInfoDTO?> GetGameInfoDTOAsync(int id)
        {
            GameInfo? entity = await _unitOfWork.GameInfoRepository.GetAsync(id, true);
            if (entity == null)
                return null;
            var dto = GameInfoDTO.Create(entity);
            _unitOfWork.DetachEntity(entity);
            return dto;
        }

        public Task<List<int>> GetGameInfoIdCollectionAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return _unitOfWork.GameInfoRepository.GetIdCollectionAsync(queryExpression);
        }

        public async Task<List<GameInfoDTO>> GetGameInfoDTOsAsync(List<int> ids, int start, int count)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IQueryable<GameInfo> entities =
                await unitOfWork.GameInfoRepository.GetGameInfosIncludeAllAsync(x => ids.Contains(x.Id), start, count);
            List<GameInfoDTO> temp = await entities.Select(x => GameInfoDTO.Create(x)).ToListAsync();
            foreach (GameInfo entity in entities)
            {
                unitOfWork.DetachEntity(entity);
            }

            return temp;
        }

        public async Task UpdateGameInfoAsync(GameInfoDTO dto)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            GameInfo entity = dto.Convert();
            GameInfo? existEntity =
                await unitOfWork.GameInfoRepository.GetAsync(x => x.GameUniqueId == entity.GameUniqueId);
            if (existEntity == null)
            {
                entity.Id = 0;
                await AddGameInfoAsync(entity);
            }
            else
            {
                entity.Id = existEntity.Id;
                unitOfWork.DetachEntity(existEntity);
                await EditGameInfo(entity);
            }
        }

        public Task GetGameInfoForEachAsync(Action<GameInfo> action, CancellationToken cancellationToken,
            SortOrder order = SortOrder.UPLOAD_TIME)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return unitOfWork.GameInfoRepository
                .GetGameInfoForEachAsync(action, cancellationToken, order);
        }


        public async Task<GameInfo> EditGameInfo(GameInfo info)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            GameInfo? currentEntity = await unitOfWork.GameInfoRepository.GetAsync(x => x.Id == info.Id);
            if (currentEntity == null)
                throw new ArgumentException($"GameInfo of id {info.Id} not found");
            List<Tag> tags = info.Tags;
            List<Staff> staffs = info.Staffs;
            List<Character> characters = info.Characters;
            List<ReleaseInfo> releaseInfos = info.ReleaseInfos;
            List<RelatedSite> relatedSites = info.RelatedSites;
            info.Tags = [];
            info.Staffs = [];
            info.Characters = [];
            info.ReleaseInfos = [];
            info.RelatedSites = [];
            // add new screenshots and remove duplicate
            info.ScreenShots.AddRange(currentEntity.ScreenShots);
            info.ScreenShots = info.ScreenShots.Distinct().ToList();
            // set default background image
            if (string.IsNullOrEmpty(info.BackgroundImageUrl) && info.ScreenShots.Count > 0)
            {
                info.BackgroundImageUrl = info.ScreenShots[0];
            }

            try
            {
                unitOfWork.BeginTransaction();
                await unitOfWork.GameInfoRepository.EditAsync(info);
                await unitOfWork.GameInfoRepository.UpdateStaffsAsync(x => x.Id == info.Id, staffs);
                await unitOfWork.GameInfoRepository.UpdateCharactersAsync(x => x.Id == info.Id, characters);
                await unitOfWork.GameInfoRepository.UpdateReleaseInfosAsync(x => x.Id == info.Id, releaseInfos);
                await unitOfWork.GameInfoRepository.UpdateRelatedSitesAsync(x => x.Id == info.Id, relatedSites);
                await unitOfWork.GameInfoRepository.UpdateTagsAsync(x => x.Id == info.Id, tags);
                await unitOfWork.SaveChangesAsync();
                unitOfWork.CommitTransaction();
            }
            catch (Exception)
            {
                unitOfWork.RollbackTransaction();
                throw;
            }

            GameInfo returnGameInfo = (await unitOfWork.GameInfoRepository.GetAsync(x => x.Id == info.Id))!;
            returnGameInfo.Tags = tags;
            returnGameInfo.Staffs = staffs;
            returnGameInfo.Characters = characters;
            returnGameInfo.ReleaseInfos = releaseInfos;
            returnGameInfo.RelatedSites = relatedSites;
            return returnGameInfo;
        }

        public Task<bool> HasGameInfo(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return _unitOfWork.GameInfoRepository.AnyAsync(queryExpression);
        }

        public async Task<List<string>> GetUniqueIdCollection(Expression<Func<GameInfo, bool>> queryExpression, int start, int count)
        {
            IQueryable<string> queryable =
                await _unitOfWork.GameInfoRepository.GetUniqueIdCollectionAsync(queryExpression, start, count);
            return queryable.ToList();
        }

        public Task<int> GetGameInfoCountAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return _unitOfWork.GameInfoRepository.CountAsync(queryExpression);
        }

        public async Task UpdateGameInfoFavoriteAsync(int id, bool isFavorite)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var entity = new GameInfo()
            {
                Id = id
            };
            AppDbContext context = unitOfWork.Context;
            context.Attach(entity);
            entity.IsFavorite = isFavorite;
            context.Entry(entity).Property(x => x.IsFavorite).IsModified = true;
            await unitOfWork.SaveChangesAsync();
            unitOfWork.DetachEntity(entity);
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
            _appSetting = unitOfWork.AppSettingRepository.GetAppSettingAsync().Result;
            unitOfWork.DetachEntity(_appSetting);
            // clear cache because the data has been changed
            _memoryCache.Dispose();
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
        }

        public Task UpdateAppSettingAsync(AppSettingDTO settingDto)
        {
            AppSetting appSetting = settingDto.Convert();
            return UpdateAppSettingAsync(appSetting);
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
            AppSetting setting = _appSetting;
            var dto = AppSettingDTO.Create(setting);
            dto.TextMappings = dto.TextMappings.DistinctBy(x => x.Id).ToList();
            dto.GuideSites = dto.GuideSites.DistinctBy(x => x.Id).ToList();
            await using FileStream fileStream = new(path, FileMode.Create);
            await JsonSerializer.SerializeAsync(fileStream, dto);
        }

        public async Task RestoreSettings(string path)
        {
            await using FileStream fileStream = new(path, FileMode.Open);
            AppSettingDTO? dto = await JsonSerializer.DeserializeAsync<AppSettingDTO>(fileStream);
            if (dto == null)
                return;
            AppSetting setting = dto.Convert();
            await UpdateAppSettingAsync(setting);
        }

        public async Task UpdateGameInfoBackgroundImageAsync(int gameInfoId, string? backgroundImage)
        {
            AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            GameInfo entity =
                await unitOfWork.GameInfoRepository.UpdateBackgroundImageAsync(gameInfoId, backgroundImage);
            await unitOfWork.SaveChangesAsync();
            unitOfWork.DetachEntity(entity);
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

        public async Task<List<Staff>> GetGameInfoStaffs(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<Staff> result = await _unitOfWork.GameInfoRepository.GetStaffsAsync(query);
            return result.ToList();
        }

        public async Task<List<Character>> GetGameInfoCharacters(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<Character> result = await _unitOfWork.GameInfoRepository.GetCharactersAsync(query);
            return result.ToList();
        }

        public async Task<List<ReleaseInfo>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query)
        {
            List<ReleaseInfo> result = await _unitOfWork.GameInfoRepository.GetGameInfoReleaseInfos(query);
            return result;
        }

        public async Task<List<RelatedSite>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<RelatedSite> result = await _unitOfWork.GameInfoRepository.GetGameInfoRelatedSites(query);
            return result.ToList();
        }

        public async Task RemoveScreenshotAsync(int gameInfoId, string url)
        {
            AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            GameInfo? entity = await unitOfWork.GameInfoRepository.RemoveScreenshotAsync(gameInfoId, url);
            if (entity == null)
                return;
            await unitOfWork.SaveChangesAsync();
            unitOfWork.DetachEntity(entity);
        }

        public async Task AddScreenshotsAsync(int gameInfoId, List<string> urls)
        {
            AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.GameInfoRepository.AddScreenshotsAsync(gameInfoId, urls);
            await unitOfWork.SaveChangesAsync();
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

        public async Task<List<string>> GetGameTagsAsync(int gameId)
        {
            IEnumerable<Tag> tags = await _unitOfWork.GameInfoRepository
                .GetTagsByIdAsync(gameId);
            IEnumerable<string> result = tags.Select(x => x.Name);
            return result.ToList();
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

        public async Task<List<string>> GetTagsAsync()
        {
            return await _unitOfWork.TagRepository.GetAllTagsAsync();
        }
    }
}