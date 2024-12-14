using GameManager.Database;
using GameManager.DB;
using GameManager.DB.Models;
using GameManager.DTOs;
using Helper;
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
        private static readonly object _DatabaseLock = new();
        private readonly IAppPathService _appPathService;

        private readonly IServiceProvider _serviceProvider;

        private readonly IUnitOfWork _unitOfWork;

        private AppSettingDTO _appSetting;

        private MemoryCache _memoryCache;

        public ConfigService(IServiceProvider serviceProvider)
        {
            _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider = serviceProvider;
            AppSetting appSettingEntity = _unitOfWork.AppSettingRepository.GetAsync().Result;
            _appSetting = AppSettingDTO.Create(appSettingEntity);
            _appPathService = serviceProvider.GetRequiredService<IAppPathService>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 200
            });
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

        public AppSettingDTO GetAppSettingDTO()
        {
            return _appSetting;
        }

        public Task<string?> GetCoverFullPath(string? coverName)
        {
            if (coverName == null)
                return Task.FromResult<string?>(null);
            string fullPath = Path.Combine(_appPathService.CoverDirPath, coverName);
            return Task.FromResult(fullPath)!;
        }

        public async Task<string?> GetScreenShotsDirPath(int gameId)
        {
            string? uniqueId =
                await _unitOfWork.GameInfoRepository.GetAsync(gameId, q => q, q => q.Select(x => x.GameUniqueId));
            if (uniqueId == null)
                return null;
            string fullPath = Path.Combine(_appPathService.ScreenShotsDirPath, uniqueId);
            return fullPath;
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
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                GameInfo? deletedInfo = null;
                await ExceptionHelper.ExecuteWithExceptionHandlingAsync(async () =>
                {
                    IGameInfoRepository gameInfoRepo = unitOfWork.GameInfoRepository;
                    string? cover = await gameInfoRepo.GetAsync(id,
                        q => q,
                        q => q.Select(x => x.CoverPath));
                    if (cover != null)
                    {
                        await DeleteCoverImage(cover);
                    }

                    deletedInfo = await unitOfWork.GameInfoRepository.DeleteAsync(id);
                    if (deletedInfo == null)
                        return;
                    string saveFilePath = Path.Combine(_appPathService.SaveFileBackupDirPath, deletedInfo.GameUniqueId);
                    ExceptionHelper.ExecuteWithExceptionHandling(() =>
                    {
                        if (Directory.Exists(saveFilePath))
                        {
                            Directory.Delete(saveFilePath, true);
                        }
                    });
                }, ex => throw ex, async () =>
                {
                    await unitOfWork.SaveChangesAsync();
                });
                if (deletedInfo == null)
                    return;

                if (_appSetting.EnableSync)
                    await AddGameInfoToPendingDeletion(deletedInfo.GameUniqueId, DateTime.UtcNow);
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task DeleteGameInfoByIdListAsync(IEnumerable<int> idList, CancellationToken cancellationToken,
            Action<int> onDeleteCallback)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            List<string> deletedGameInfoUniqueId = [];
            await Parallel.ForEachAsync(idList, cancellationToken, async (id, token) =>
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await using AsyncServiceScope subScope = _serviceProvider.CreateAsyncScope();
                    IUnitOfWork subUnitOfWork = subScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    IGameInfoRepository gameInfoRepo = subUnitOfWork.GameInfoRepository;
                    string? cover = await gameInfoRepo.GetAsync(id,
                        q => q,
                        q => q.Select(x => x.CoverPath));
                    if (cover != null)
                    {
                        await DeleteCoverImage(cover);
                    }

                    Monitor.Enter(_DatabaseLock);
                    GameInfo? deletedEntity = unitOfWork.GameInfoRepository.DeleteAsync(id).Result;
                    if (deletedEntity == null)
                        return;
                    deletedGameInfoUniqueId.Add(deletedEntity.GameUniqueId);
                    string saveFileDir =
                        Path.Combine(_appPathService.SaveFileBackupDirPath, deletedEntity.GameUniqueId);
                    ExceptionHelper.ExecuteWithExceptionHandling(() =>
                    {
                        if (Directory.Exists(saveFileDir))
                        {
                            Directory.Delete(saveFileDir, true);
                        }
                    });
                }
                finally
                {
                    if (Monitor.IsEntered(_DatabaseLock))
                        Monitor.Exit(_DatabaseLock);
                    onDeleteCallback(id);
                }
            });
            await unitOfWork.SaveChangesAsync();

            if (_appSetting.EnableSync)
                await AddGameInfosToPendingDeletion(deletedGameInfoUniqueId, DateTime.UtcNow);
        }

        public async Task<GameInfoDTO> AddGameInfoAsync(GameInfoDTO dto, bool generateUniqueId = true)
        {
            GameInfo gameInfo = dto.Convert();
            gameInfo.Id = 0;
            gameInfo.LaunchOptionId = 0;
            gameInfo.LaunchOption ??= new LaunchOption();
            gameInfo.LaunchOption.Id = 0;
            GameInfo? entity = await AddGameInfoInternalAsync(gameInfo, generateUniqueId);
            if (entity == null)
                throw new InvalidOperationException("Add game info failed");
            GameInfoDTO? resultDto =
                await GetGameInfoDTOAsync(x => x.Id == entity.Id, q => q.Include(x => x.LaunchOption));
            if (resultDto == null)
                throw new InvalidOperationException("Add game info failed");
            return resultDto;
        }

        public async Task<GameInfoDTO?> GetGameInfoDTOAsync(Expression<Func<GameInfo, bool>> queryExpression,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeQuery)
        {
            GameInfo? entity = await _unitOfWork.GameInfoRepository.GetAsync(queryExpression,
                includeQuery);
            if (entity == null)
                return null;
            _unitOfWork.DetachEntity(entity);
            var dto = GameInfoDTO.Create(entity);
            return dto;
        }

        public async Task<GameInfoDTO?> GetGameInfoBaseDTOAsync(int id)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            GameInfo? entity = await unitOfWork.GameInfoRepository.GetAsync(x => x.Id == id,
                q => q.Include(x => x.LaunchOption));
            if (entity == null)
                return null;

            var dto = GameInfoDTO.Create(entity);
            unitOfWork.DetachEntity(entity);
            return dto;
        }

        public async Task<List<int>> GetGameInfoIdCollectionAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            IEnumerable<int> result = await _unitOfWork.GameInfoRepository.GetManyAsync(queryExpression, q => q, q =>
            {
                return q.Select(x => x.Id);
            });
            return result.ToList();
        }

        public async Task<List<GameInfoDTO>> GetGameInfoDTOsAsync(List<int> ids, int start, int count,
            Func<IQueryable<GameInfo>, IQueryable<GameInfo>>? includeFunc = null)
        {
            IEnumerable<GameInfo> entities = await _unitOfWork.GameInfoRepository.GetManyAsync(
                x => ids.Contains(x.Id),
                includeFunc);
            return entities.Select(GameInfoDTO.Create).ToList();
        }

        public async Task<List<GameInfoDTO>> GetGameInfoIncludeAllDTOsAsync(List<int> ids, int start, int count)
        {
            IEnumerable<GameInfo> gameInfos = await GetGameInfoIncludeAllAsync(x => ids.Contains(x.Id));
            return gameInfos.Skip(start).Take(count).Select(GameInfoDTO.Create).ToList();
        }

        public async Task<GameInfoDTO> UpdateGameInfoAsync(GameInfoDTO dto)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            GameInfo entity = dto.Convert();
            GameInfo? existEntity =
                await unitOfWork.GameInfoRepository.GetAsync(x => x.GameUniqueId == entity.GameUniqueId);
            GameInfo updatedEntity;
            if (existEntity == null)
            {
                entity.Id = 0;
                updatedEntity = await AddGameInfoInternalAsync(entity);
            }
            else
            {
                entity.Id = existEntity.Id;
                unitOfWork.DetachEntity(existEntity);
                updatedEntity = await EditGameInfoInternalAsync(entity);
            }

            return GameInfoDTO.Create(updatedEntity);
        }

        public async Task GetGameInfoForEachAsync(Action<GameInfoDTO> action, CancellationToken cancellationToken)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IEnumerable<GameInfo> entities =
                await unitOfWork.GameInfoRepository.GetManyAsync(x => true, q => q.Include(x => x.LaunchOption));
            foreach (GameInfo entity in entities)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var dto = GameInfoDTO.Create(entity);
                action(dto);
            }
        }

        public Task<bool> HasGameInfo(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return _unitOfWork.GameInfoRepository.AnyAsync(queryExpression);
        }

        public async Task<List<string>> GetUniqueIdCollection(Expression<Func<GameInfo, bool>> queryExpression,
            int start, int count)
        {
            IEnumerable<string> result = await _unitOfWork.GameInfoRepository.GetManyAsync(
                queryExpression,
                q => q,
                q =>
                {
                    return q.Select(x => x.GameUniqueId);
                });
            return result.ToList();
        }

        public Task<int> GetGameInfoCountAsync(Expression<Func<GameInfo, bool>> queryExpression)
        {
            return _unitOfWork.GameInfoRepository.CountAsync(queryExpression);
        }

        public async Task UpdateGameInfoFavoriteAsync(int id, bool isFavorite)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var entity = new GameInfo
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
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task UpdateGameInfoSyncStatusAsync(List<int> ids, bool isSyncEnable)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var entities = ids.Select(x => new GameInfo
                {
                    Id = x
                }).ToList();
                AppDbContext context = unitOfWork.Context;
                context.AttachRange(entities);
                foreach (GameInfo entity in entities)
                {
                    entity.EnableSync = isSyncEnable;
                    context.Entry(entity).Property(x => x.EnableSync).IsModified = true;
                }

                await unitOfWork.SaveChangesAsync();
                foreach (GameInfo entity in entities)
                {
                    unitOfWork.DetachEntity(entity);
                }
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task UpdateAppSettingAsync(AppSettingDTO setting)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                AppSetting updateAppSetting = setting.Convert();
                await unitOfWork.AppSettingRepository.UpdateAsync(updateAppSetting);
                await unitOfWork.SaveChangesAsync();
                _appSetting = GetAppSettingDTO();
                unitOfWork.DetachEntity(updateAppSetting);
                // clear cache because the data has been changed
                _memoryCache.Dispose();
                _memoryCache = new MemoryCache(new MemoryCacheOptions
                {
                    SizeLimit = 200
                });
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task<List<LibraryDTO>> GetLibrariesAsync(CancellationToken cancellationToken)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IEnumerable<Library> entities = await unitOfWork.LibraryRepository.GetManyAsync(x => true);
            return entities.Select(LibraryDTO.Create).ToList();
        }

        public async Task AddLibraryAsync(LibraryDTO library)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                Library updateLibrary = library.Convert();
                await unitOfWork.LibraryRepository.AddAsync(updateLibrary);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task DeleteLibraryByIdAsync(int id)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.LibraryRepository.DeleteAsync(id);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public Task<bool> CheckExePathExist(string path)
        {
            return _unitOfWork.GameInfoRepository.AnyAsync(x => x.ExePath == path);
        }

        public async Task UpdateLastPlayedByIdAsync(int id, DateTime time)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.GameInfoRepository.UpdatePropertiesAsync(new GameInfo
                {
                    Id = id,
                    LastPlayed = time
                }, x => x.LastPlayed);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task UpdatePlayTimeAsync(int gameId, TimeSpan timeToAdd)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IGameInfoRepository gameInfoRepository = unitOfWork.GameInfoRepository;
                double playTime = await gameInfoRepository.GetAsync(gameId,
                    q => q,
                    q => q.Select(x => x.PlayTime));
                await gameInfoRepository.UpdatePropertiesAsync(new GameInfo
                {
                    Id = gameId,
                    PlayTime = playTime + timeToAdd.TotalMinutes
                }, x => x.PlayTime);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task BackupSettings(string path)
        {
            AppSettingDTO dto = _appSetting;
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
            await UpdateAppSettingAsync(dto);
        }

        public async Task UpdateGameInfoBackgroundImageAsync(int gameInfoId, string? backgroundImage)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.GameInfoRepository.UpdatePropertiesAsync(new GameInfo
                {
                    Id = gameInfoId,
                    BackgroundImageUrl = backgroundImage
                }, x => x.BackgroundImageUrl);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task<IEnumerable<StaffRoleDTO>> GetStaffRolesAsync()
        {
            IEnumerable<StaffRole> roles = await _unitOfWork.StaffRoleRepository.GetAsync(_ => true);
            return roles.Select(StaffRoleDTO.Create);
        }

        public async Task<StaffDTO?> GetStaffAsync(Expression<Func<Staff, bool>> query)
        {
            Staff? entity = await _unitOfWork.StaffRepository.GetAsync(query);
            if (entity == null)
                return null;
            return StaffDTO.Create(entity);
        }

        public async Task<List<StaffDTO>> GetGameInfoStaffDTOs(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<Staff>? result = await _unitOfWork.GameInfoRepository.GetAsync(query,
                q => q.Include(x => x.Staffs).ThenInclude(x => x.StaffRole)
                , q => q.Select(x => x.Staffs).Select(x => x));
            return result == null ? [] : result.Select(StaffDTO.Create).ToList();
        }

        public async Task<List<CharacterDTO>> GetGameInfoCharacters(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<Character>? result = await _unitOfWork.GameInfoRepository.GetAsync(query,
                q => q.Include(x => x.Characters),
                q => q.Select(x => x.Characters));
            return result == null ? [] : result.Select(CharacterDTO.Create).ToList();
        }

        public async Task<List<ReleaseInfoDTO>> GetGameInfoReleaseInfos(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<ReleaseInfo>? result = await _unitOfWork.GameInfoRepository.GetAsync(query,
                q => q.Include(x => x.ReleaseInfos).ThenInclude(x => x.ExternalLinks),
                q => q.Select(x => x.ReleaseInfos));
            return result == null ? [] : result.Select(ReleaseInfoDTO.Create).ToList();
        }

        public async Task<List<RelatedSiteDTO>> GetGameInfoRelatedSites(Expression<Func<GameInfo, bool>> query)
        {
            IEnumerable<RelatedSite>? result = await _unitOfWork.GameInfoRepository.GetAsync(query,
                q => q.Include(x => x.RelatedSites),
                q => q.Select(x => x.RelatedSites));
            return result == null ? [] : result.Select(RelatedSiteDTO.Create).ToList();
        }

        public async Task RemoveScreenshotsAsync(int gameInfoId, List<string> urls)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IGameInfoRepository gameInfoRepo = unitOfWork.GameInfoRepository;
                GameInfo? entity = await gameInfoRepo.GetAsync(gameInfoId);
                if (entity == null)
                    return;
                unitOfWork.DetachEntity(entity);
                entity.ScreenShots.RemoveAll(urls.Contains);
                await gameInfoRepo.UpdatePropertiesAsync(entity, x => x.ScreenShots);
                await unitOfWork.SaveChangesAsync();
                unitOfWork.DetachEntity(entity);
                foreach (string filePath in from url in urls
                         where !url.IsHttpLink() && !url.StartsWith("cors://")
                         let screenShotDirPath = Path.Combine(_appPathService.ScreenShotsDirPath, entity.GameUniqueId)
                         select Path.Combine(screenShotDirPath, url)
                         into filePath
                         where File.Exists(filePath)
                         select filePath)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task AddScreenshotsAsync(int gameInfoId, List<string> urls)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IGameInfoRepository gameInfoRepo = unitOfWork.GameInfoRepository;
                GameInfo? entity = await gameInfoRepo.GetAsync(gameInfoId);
                if (entity == null)
                    return;
                entity.ScreenShots.AddRange(urls);
                entity.ScreenShots = entity.ScreenShots.Distinct().ToList();
                await gameInfoRepo.UpdatePropertiesAsync(entity, x => x.ScreenShots);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task AddScreenshotsFromFilesAsync(int gameInfoId, List<string> filePaths)
        {
            AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            string? gameInfoUniqueId = await unitOfWork.GameInfoRepository.GetAsync(x => x.Id == gameInfoId,
                query => query
                , query => query.Select(x => x.GameUniqueId));
            if (string.IsNullOrEmpty(gameInfoUniqueId))
                return;
            string targetDir = Path.Combine(_appPathService.ScreenShotsDirPath, gameInfoUniqueId);
            Directory.CreateDirectory(targetDir);
            List<string> errors = [];
            List<string> addedFiles = [];
            foreach (string file in filePaths)
            {
                if (!File.Exists(file))
                    continue;
                string extension = Path.GetExtension(file);
                string newFileName = Guid.NewGuid() + extension;
                string targetFilePath = Path.Combine(targetDir, newFileName);
                try
                {
                    File.Copy(file , targetFilePath);
                    addedFiles.Add(Path.GetFileName(targetFilePath));
                }
                catch (Exception e)
                {
                    errors.Add("Error while copy file: " + file + " to " + targetFilePath + " " +
                               e.Message);
                }
            }

            string errorMessage = "";
            if (errors.Count > 0)
                errorMessage = string.Join("\n", errors);
            
            // if add to database failed, delete all added files
            try
            {
                await AddScreenshotsAsync(gameInfoId, addedFiles);
            }
            catch (Exception)
            {
                foreach (string file in addedFiles)
                {
                    string targetFilePath = Path.Combine(targetDir, file);
                    try
                    {
                        File.Delete(targetFilePath);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

                throw;
            }

            if (errorMessage.Length > 0)
                throw new InvalidOperationException(errorMessage);
        }

        public async Task<List<PendingGameInfoDeletionDTO>> GetPendingGameInfoDeletionUniqueIdsAsync()
        {
            IEnumerable<PendingGameInfoDeletion> pendingList =
                await _unitOfWork.PendingGameInfoDeletionRepository.GetManyAsync(x => true);
            return pendingList.Select(PendingGameInfoDeletionDTO.Create).ToList();
        }

        public async Task RemovePendingGameInfoDeletionsAsync(
            List<PendingGameInfoDeletionDTO> pendingGameInfoDeletionDTOs)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                AsyncServiceScope asyncScope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = asyncScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var hashSet = pendingGameInfoDeletionDTOs.Select(x => x.GameUniqueId).ToHashSet();
                IEnumerable<PendingGameInfoDeletion> pendingDeleteEntity =
                    await unitOfWork.PendingGameInfoDeletionRepository.GetManyAsync(x =>
                        hashSet.Contains(x.GameInfoUniqueId));
                foreach (PendingGameInfoDeletion deletion in pendingDeleteEntity)
                {
                    await unitOfWork.PendingGameInfoDeletionRepository.DeleteAsync(deletion.Id);
                }
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        public async Task<TextMappingDTO?> SearchTextMappingByOriginalText(string original)
        {
            string key = "TextMapping::" + original;
            if (_memoryCache.TryGetValue(key, out TextMappingDTO? mapping))
            {
                return mapping;
            }

            TextMapping? entity = await _unitOfWork.AppSettingRepository.SearchTextMappingByOriginalText(original);
            if (entity == null)
            {
                return null;
            }

            mapping = TextMappingDTO.Create(entity);
            _memoryCache.Set(key, mapping, new MemoryCacheEntryOptions
            {
                Size = 1
            });
            return mapping;
        }

        public async Task<List<string>> GetGameTagsAsync(int gameId)
        {
            List<Tag>? tagEntities = await _unitOfWork.GameInfoRepository.GetAsync(gameId,
                q => q.Include(x => x.Tags),
                q => q.Select(x => x.Tags));
            return tagEntities == null ? [] : tagEntities.Select(x => x.Name).ToList();
        }

        public async Task<bool> CheckGameInfoHasTag(int gameId, string tagName)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            Tag? tag = await unitOfWork.TagRepository.GetAsync(x => x.Name == tagName);
            if (tag == null)
            {
                return false;
            }

            bool hasTag = await _unitOfWork.GameInfoTagRepository.CheckGameHasTag(tag.Id, gameId);
            return hasTag;
        }

        public async Task<List<string>> GetTagsAsync()
        {
            IEnumerable<Tag> tags = await _unitOfWork.TagRepository.GetManyAsync(x => true);
            return tags.Select(x => x.Name).ToList();
        }

        private async Task AddGameInfosToPendingDeletion(List<string> gameInfoUniqueIds, DateTime deletedTime)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var deletions = gameInfoUniqueIds.Select(x => new PendingGameInfoDeletion
                {
                    GameInfoUniqueId = x,
                    DeletionDate = deletedTime
                }).ToList();

                await unitOfWork.PendingGameInfoDeletionRepository.AddManyAsync(deletions);
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        private async Task AddGameInfoToPendingDeletion(string gameInfoUniqueId, DateTime deletedTime)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.PendingGameInfoDeletionRepository.AddAsync(new PendingGameInfoDeletion
                {
                    GameInfoUniqueId = gameInfoUniqueId,
                    DeletionDate = deletedTime
                });
                await unitOfWork.SaveChangesAsync();
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        private async Task<GameInfo> AddGameInfoInternalAsync(GameInfo info, bool generateUniqueId = true)
        {
            Monitor.Enter(_DatabaseLock);
            try
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
                    await unitOfWork.GameInfoRepository.UpdateStaffsAsync(gameInfoEntity, staffs);
                    await unitOfWork.GameInfoRepository.UpdateCharactersAsync(gameInfoEntity, characters);
                    await unitOfWork.GameInfoRepository.UpdateReleaseInfosAsync(gameInfoEntity, releaseInfos);
                    await unitOfWork.GameInfoRepository.UpdateRelatedSitesAsync(gameInfoEntity, relatedSites);
                    await unitOfWork.GameInfoRepository.UpdateTagsAsync(gameInfoEntity, tags);
                    await unitOfWork.SaveChangesAsync();
                    unitOfWork.CommitTransaction();

                    return gameInfoEntity;
                }
                catch (Exception)
                {
                    unitOfWork.RollbackTransaction();
                    throw;
                }
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }

        private async Task<IEnumerable<GameInfo>> GetGameInfoIncludeAllAsync(Expression<Func<GameInfo, bool>> query)
        {
            AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var entities =
                (await unitOfWork.GameInfoRepository.GetManyAsync(query, q => q.Include(x => x.LaunchOption))).ToList();

            foreach (GameInfo entity in entities)
            {
                entity.Staffs = (await unitOfWork.GameInfoRepository.GetStaffs(entity.Id)).ToList();
                entity.Characters = (await unitOfWork.GameInfoRepository.GetCharacters(entity.Id)).ToList();
                entity.RelatedSites = (await unitOfWork.GameInfoRepository.GetRelatedSites(entity.Id)).ToList();
                entity.ReleaseInfos = (await unitOfWork.GameInfoRepository.GetReleaseInfos(entity.Id)).ToList();
                entity.Tags = (await unitOfWork.GameInfoRepository.GetTags(entity.Id)).ToList();
            }

            return entities;
        }


        private async Task<GameInfo> EditGameInfoInternalAsync(GameInfo info)
        {
            Monitor.Enter(_DatabaseLock);
            try
            {
                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                GameInfo? currentEntity = await unitOfWork.GameInfoRepository.GetAsync(info.Id,
                    q =>
                    {
                        return q.Include(x => x.LaunchOption);
                    });

                if (currentEntity == null)
                    throw new ArgumentException($"GameInfo of id {info.Id} not found");

                // set default background image
                if (string.IsNullOrEmpty(info.BackgroundImageUrl) && info.ScreenShots.Count > 0)
                {
                    info.BackgroundImageUrl = info.ScreenShots[0];
                }

                try
                {
                    unitOfWork.BeginTransaction();
                    await unitOfWork.GameInfoRepository.UpdateStaffsAsync(currentEntity, info.Staffs);
                    await unitOfWork.GameInfoRepository.UpdateCharactersAsync(currentEntity, info.Characters);
                    await unitOfWork.GameInfoRepository.UpdateReleaseInfosAsync(currentEntity, info.ReleaseInfos);
                    await unitOfWork.GameInfoRepository.UpdateRelatedSitesAsync(currentEntity, info.RelatedSites);
                    await unitOfWork.GameInfoRepository.UpdateTagsAsync(currentEntity, info.Tags);
                    await unitOfWork.GameInfoRepository.UpdateLaunchOption(currentEntity,
                        info.LaunchOption ?? new LaunchOption());
                    currentEntity.ScreenShots.AddRange(info.ScreenShots);
                    currentEntity.ScreenShots = currentEntity.ScreenShots.Distinct().ToList();
                    await unitOfWork.GameInfoRepository.UpdateAsync(currentEntity, info);
                    await unitOfWork.SaveChangesAsync();
                    unitOfWork.CommitTransaction();
                }
                catch (Exception)
                {
                    unitOfWork.RollbackTransaction();
                    throw;
                }

                unitOfWork.DetachEntity(currentEntity);
                return currentEntity;
            }
            finally
            {
                if (Monitor.IsEntered(_DatabaseLock))
                    Monitor.Exit(_DatabaseLock);
            }
        }
    }
}