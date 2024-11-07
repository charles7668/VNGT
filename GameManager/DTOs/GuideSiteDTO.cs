using GameManager.DB.Models;
using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace GameManager.DTOs
{
    public class GuideSiteDTO : IConvertable<GuideSite>
    {
        [JsonIgnore]
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string? Name { get; set; }

        [UsedImplicitly]
        public string? SiteUrl { get; set; }

        public static GuideSiteDTO Create(GuideSite guideSite)
        {
            return new GuideSiteDTO
            {
                Id = guideSite.Id,
                Name = guideSite.Name,
                SiteUrl = guideSite.SiteUrl
            };
        }

        public GuideSite Convert()
        {
            return new GuideSite
            {
                Id = Id,
                Name = Name,
                SiteUrl = SiteUrl
            };
        }
    }
}