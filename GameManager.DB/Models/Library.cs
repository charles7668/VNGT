using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    public class Library
    {
        public int Id { get; set; }

        [MaxLength(260)]
        public string? FolderPath { get; set; }
    }
}