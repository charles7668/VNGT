using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class GameInfoDTO : IConvertable<GameInfo>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string GameUniqueId { get; set; } = Guid.NewGuid().ToString();

        [UsedImplicitly]
        public string? GameInfoFetchId { get; set; }

        [UsedImplicitly]
        public string? GameName { get; set; }

        [UsedImplicitly]
        public string? GameChineseName { get; set; }

        [UsedImplicitly]
        public string? GameEnglishName { get; set; }

        [UsedImplicitly]
        public string? Developer { get; set; }

        [UsedImplicitly]
        public string? ExePath { get; set; }

        [UsedImplicitly]
        public string? SaveFilePath { get; set; }

        [UsedImplicitly]
        public string? ExeFile { get; set; }

        [UsedImplicitly]
        public string? CoverPath { get; set; }

        [UsedImplicitly]
        public string? Description { get; set; }

        [UsedImplicitly]
        public DateTime? ReleaseDate { get; set; }

        [UsedImplicitly]
        public LaunchOptionDTO? LaunchOption { get; set; }

        [UsedImplicitly]
        public DateTime? UploadTime { get; set; }

        [UsedImplicitly]
        public bool IsFavorite { get; set; }

        [UsedImplicitly]
        public DateTime? LastPlayed { get; set; }

        [UsedImplicitly]
        public List<StaffDTO> Staffs { get; set; } = [];

        [UsedImplicitly]
        public List<CharacterDTO> Characters { get; set; } = [];

        [UsedImplicitly]
        public List<ReleaseInfoDTO> ReleaseInfos { get; set; } = [];

        [UsedImplicitly]
        public List<RelatedSiteDTO> RelatedSites { get; set; } = [];

        [UsedImplicitly]
        public List<TagDTO> Tags { get; set; } = [];

        [UsedImplicitly]
        public List<string> ScreenShots { get; set; } = [];

        [UsedImplicitly]
        public string? BackgroundImageUrl { get; set; } = string.Empty;

        [UsedImplicitly]
        public DateTime UpdatedTime { get; set; } = DateTime.MinValue;

        [UsedImplicitly]
        public bool EnableSync { get; set; }

        [UsedImplicitly]
        public double PlayTime { get; set; }

        public GameInfo Convert()
        {
            return new GameInfo
            {
                Id = Id,
                GameUniqueId = GameUniqueId,
                GameInfoFetchId = GameInfoFetchId,
                GameName = GameName,
                GameChineseName = GameChineseName,
                GameEnglishName = GameEnglishName,
                Developer = Developer,
                ExePath = ExePath,
                SaveFilePath = SaveFilePath,
                ExeFile = ExeFile,
                CoverPath = CoverPath,
                Description = Description,
                ReleaseDate = ReleaseDate,
                LaunchOption = LaunchOption?.Convert(),
                UploadTime = UploadTime,
                IsFavorite = IsFavorite,
                LastPlayed = LastPlayed,
                Staffs = Staffs.Select(x => x.Convert()).ToList(),
                Characters = Characters.Select(x => x.Convert()).ToList(),
                ReleaseInfos = ReleaseInfos.Select(x => x.Convert()).ToList(),
                RelatedSites = RelatedSites.Select(x => x.Convert()).ToList(),
                Tags = Tags.Select(x => x.Convert()).ToList(),
                ScreenShots = ScreenShots,
                BackgroundImageUrl = BackgroundImageUrl,
                UpdatedTime = UpdatedTime,
                EnableSync = EnableSync,
                PlayTime = PlayTime
            };
        }

        public static GameInfoDTO Create(GameInfo gameInfo)
        {
            return new GameInfoDTO
            {
                Id = gameInfo.Id,
                GameUniqueId = gameInfo.GameUniqueId,
                GameInfoFetchId = gameInfo.GameInfoFetchId,
                GameName = gameInfo.GameName,
                GameChineseName = gameInfo.GameChineseName,
                GameEnglishName = gameInfo.GameEnglishName,
                Developer = gameInfo.Developer,
                ExePath = gameInfo.ExePath,
                SaveFilePath = gameInfo.SaveFilePath,
                ExeFile = gameInfo.ExeFile,
                CoverPath = gameInfo.CoverPath,
                Description = gameInfo.Description,
                ReleaseDate = gameInfo.ReleaseDate,
                LaunchOption = gameInfo.LaunchOption is null ? null : LaunchOptionDTO.Create(gameInfo.LaunchOption),
                UploadTime = gameInfo.UploadTime,
                IsFavorite = gameInfo.IsFavorite,
                LastPlayed = gameInfo.LastPlayed,
                Staffs = gameInfo.Staffs.Select(StaffDTO.Create).ToList(),
                Characters = gameInfo.Characters.Select(CharacterDTO.Create).ToList(),
                ReleaseInfos = gameInfo.ReleaseInfos.Select(ReleaseInfoDTO.Create).ToList(),
                RelatedSites = gameInfo.RelatedSites.Select(RelatedSiteDTO.Create).ToList(),
                Tags = gameInfo.Tags.Select(TagDTO.Create).ToList(),
                ScreenShots = gameInfo.ScreenShots,
                BackgroundImageUrl = gameInfo.BackgroundImageUrl,
                UpdatedTime = gameInfo.UpdatedTime,
                EnableSync = gameInfo.EnableSync,
                PlayTime = gameInfo.PlayTime
            };
        }
    }
}