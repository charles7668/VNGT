using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class ExternalLinkDTO : IConvertable<ExternalLink>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string? Url { get; set; }

        [UsedImplicitly]
        public string? Name { get; set; }

        [UsedImplicitly]
        public string? Label { get; set; }

        public ExternalLink Convert()
        {
            return new ExternalLink
            {
                Id = Id,
                Url = Url,
                Name = Name,
                Label = Label
            };
        }

        public static ExternalLinkDTO Create(ExternalLink externalLink)
        {
            return new ExternalLinkDTO
            {
                Id = externalLink.Id,
                Url = externalLink.Url,
                Name = externalLink.Name,
                Label = externalLink.Label
            };
        }
    }
}