using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class TagDTO : IConvertable<Tag>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string Name { get; set; } = null!;

        public Tag Convert()
        {
            return new Tag
            {
                Id = Id,
                Name = Name
            };
        }

        public static TagDTO Create(Tag tag)
        {
            return new TagDTO
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }
    }
}