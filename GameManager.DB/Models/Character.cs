namespace GameManager.DB.Models
{
    public class Character
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? OriginalName { get; set; }

        public List<string> Alias { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? Age { get; set; }

        public string? Sex { get; set; }

        public string? Birthday { get; set; }

        public string? BloodType { get; set; }
        
        public int GameInfoId { get; set; }
        public GameInfo GameInfo { get; set; } = null!;
    }
}