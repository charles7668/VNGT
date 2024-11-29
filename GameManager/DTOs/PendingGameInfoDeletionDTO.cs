using GameManager.DB.Models;
using JetBrains.Annotations;

namespace GameManager.DTOs
{
    public class PendingGameInfoDeletionDTO : IConvertable<PendingGameInfoDeletion>
    {
        [UsedImplicitly]
        public string GameUniqueId { get; set; } = string.Empty;

        [UsedImplicitly]
        public DateTime DeletionDate { get; set; }

        public PendingGameInfoDeletion Convert()
        {
            return new PendingGameInfoDeletion
            {
                GameInfoUniqueId = GameUniqueId,
                DeletionDate = DeletionDate
            };
        }

        [UsedImplicitly]
        public static PendingGameInfoDeletionDTO Create(PendingGameInfoDeletion pendingGameInfoDeletion)
        {
            return new PendingGameInfoDeletionDTO
            {
                GameUniqueId = pendingGameInfoDeletion.GameInfoUniqueId,
                DeletionDate = pendingGameInfoDeletion.DeletionDate
            };
        }
    }
}