using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.Database
{
    public interface IGameInfoRepository : IBaseRepository<GameInfo>
    {
        [UsedImplicitly]
        Task UpdateStaffsAsync(GameInfo entity, List<Staff> staffs);

        [UsedImplicitly]
        Task UpdateCharactersAsync(GameInfo entity, List<Character> characters);

        [UsedImplicitly]
        Task UpdateReleaseInfosAsync(GameInfo entity, List<ReleaseInfo> releaseInfos);

        [UsedImplicitly]
        Task UpdateRelatedSitesAsync(GameInfo entity, List<RelatedSite> relatedSites);

        [UsedImplicitly]
        Task UpdateTagsAsync(GameInfo entity, List<Tag> tags);

        [UsedImplicitly]
        Task UpdateLaunchOption(GameInfo entity, LaunchOption launchOption);
    }
}