using GameManager.DB.Models;

namespace GameManager.DTOs
{
    public class GuideSiteDTO : IConvertable<GuideSite>
    {
        public int Id { get; set; }

        public string? Name { get; set; }

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