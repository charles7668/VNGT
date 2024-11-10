using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class PendingGameInfoDeletionDTO
    {
        [UsedImplicitly]
        public string GameUniqueId { get; set; } = string.Empty;

        [UsedImplicitly]
        public DateTime DeletionDate { get; set; }
    }
}