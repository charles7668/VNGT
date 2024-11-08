using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class RelatedSiteDTO : IConvertable<RelatedSite>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string? Url { get; set; }

        [UsedImplicitly]
        public string? Name { get; set; }

        public RelatedSite Convert()
        {
            return new RelatedSite
            {
                Id = Id,
                Url = Url,
                Name = Name
            };
        }

        public static RelatedSiteDTO Create(RelatedSite relatedSite)
        {
            return new RelatedSiteDTO
            {
                Id = relatedSite.Id,
                Url = relatedSite.Url,
                Name = relatedSite.Name
            };
        }
    }
}