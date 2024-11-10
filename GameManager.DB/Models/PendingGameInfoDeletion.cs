using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GameManager.DB.Models
{
    [Index(nameof(GameInfoUniqueId), IsUnique = true)]
    public class PendingGameInfoDeletion
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [MaxLength(100)]
        [UsedImplicitly]
        public string GameInfoUniqueId { get; set; } = string.Empty;

        [UsedImplicitly]
        public DateTime DeletionDate { get; set; }
    }
}