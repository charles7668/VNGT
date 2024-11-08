using GameManager.DB.Enums;
using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class ReleaseInfoDTO : IConvertable<ReleaseInfo>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string ReleaseName { get; set; } = string.Empty;

        [UsedImplicitly]
        public string ReleaseLanguage { get; set; } = string.Empty;

        [UsedImplicitly]
        public DateTime ReleaseDate { get; set; }

        [UsedImplicitly]
        public List<PlatformEnum> Platforms { get; set; } = [];

        [UsedImplicitly]
        public int AgeRating { get; set; }

        [UsedImplicitly]
        public List<ExternalLinkDTO> ExternalLinks { get; set; } = [];

        public ReleaseInfo Convert()
        {
            return new ReleaseInfo
            {
                Id = Id,
                ReleaseName = ReleaseName,
                ReleaseLanguage = ReleaseLanguage,
                ReleaseDate = ReleaseDate,
                Platforms = Platforms,
                AgeRating = AgeRating,
                ExternalLinks = ExternalLinks.Select(externalLink => externalLink.Convert()).ToList()
            };
        }

        public static ReleaseInfoDTO Create(ReleaseInfo releaseInfo)
        {
            return new ReleaseInfoDTO
            {
                Id = releaseInfo.Id,
                ReleaseName = releaseInfo.ReleaseName,
                ReleaseLanguage = releaseInfo.ReleaseLanguage,
                ReleaseDate = releaseInfo.ReleaseDate,
                Platforms = releaseInfo.Platforms,
                AgeRating = releaseInfo.AgeRating,
                ExternalLinks = releaseInfo.ExternalLinks.Select(ExternalLinkDTO.Create).ToList()
            };
        }
    }
}