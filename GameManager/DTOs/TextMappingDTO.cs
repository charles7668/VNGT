using GameManager.DB.Models;
using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace GameManager.DTOs
{
    public class TextMappingDTO : IConvertable<TextMapping>
    {
        [JsonIgnore]
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string? Original { get; set; }

        [UsedImplicitly]
        public string? Replace { get; set; }

        public TextMapping Convert()
        {
            return new TextMapping
            {
                Id = Id,
                Original = Original,
                Replace = Replace
            };
        }

        public static TextMappingDTO Create(TextMapping textMapping)
        {
            return new TextMappingDTO
            {
                Id = textMapping.Id,
                Original = textMapping.Original,
                Replace = textMapping.Replace
            };
        }
    }
}