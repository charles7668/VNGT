using JetBrains.Annotations;
using Library = GameManager.DB.Models.Library;

namespace GameManager.DTOs
{
    public class LibraryDTO : IConvertable<Library>
    {
        [UsedImplicitly]
        public int Id { get; set; }

        [UsedImplicitly]
        public string? FolderPath { get; set; }

        [UsedImplicitly]
        public DateTime UpdatedTime { get; set; } = DateTime.MinValue;

        public Library Convert()
        {
            return new Library
            {
                Id = Id,
                FolderPath = FolderPath,
                UpdatedTime = UpdatedTime
            };
        }

        public static LibraryDTO Create(Library library)
        {
            return new LibraryDTO
            {
                Id = library.Id,
                FolderPath = library.FolderPath,
                UpdatedTime = library.UpdatedTime
            };
        }
    }
}