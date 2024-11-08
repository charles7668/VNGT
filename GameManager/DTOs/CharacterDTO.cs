using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class CharacterDTO : IConvertable<Character>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string? Name { get; set; }

        [UsedImplicitly]
        public string? OriginalName { get; set; }

        [UsedImplicitly]
        public List<string> Alias { get; set; } = [];

        [UsedImplicitly]
        public string? Description { get; set; }

        [UsedImplicitly]
        public string? ImageUrl { get; set; }

        [UsedImplicitly]
        public string? Age { get; set; }

        [UsedImplicitly]
        public string? Sex { get; set; }

        [UsedImplicitly]
        public string? Birthday { get; set; }

        [UsedImplicitly]
        public string? BloodType { get; set; }

        public Character Convert()
        {
            return new Character
            {
                Id = Id,
                Name = Name,
                OriginalName = OriginalName,
                Alias = Alias,
                Description = Description,
                ImageUrl = ImageUrl,
                Age = Age,
                Birthday = Birthday,
                BloodType = BloodType,
                Sex = Sex
            };
        }

        public static CharacterDTO Create(Character character)
        {
            return new CharacterDTO
            {
                Id = character.Id,
                Name = character.Name,
                OriginalName = character.OriginalName,
                Alias = character.Alias,
                Description = character.Description,
                ImageUrl = character.ImageUrl,
                Age = character.Age,
                Birthday = character.Birthday,
                BloodType = character.BloodType,
                Sex = character.Sex
            };
        }
    }
}