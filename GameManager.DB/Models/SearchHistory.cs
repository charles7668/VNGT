using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }

        public string SearchText { get; set; } = string.Empty;

        public DateTime SearchTime { get; set; } = DateTime.UtcNow;
    }
}