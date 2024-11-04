using GameManager.DB.Models;

namespace GameManager.DTOs
{
    public class TextMappingDTO : IConvertable<TextMapping>
    {
        public int Id { get; set; }

        public string? Original { get; set; }

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