using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface ILibraryRepository
    {
        public Task<List<Library>> GetLibrariesAsync();

        public Task AddAsync(Library library);

        public Task DeleteByIdAsync(int id);
    }
}