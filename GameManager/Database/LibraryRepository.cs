using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    internal class LibraryRepository(AppDbContext dbContext) : ILibraryRepository
    {
        public Task<List<Library>> GetLibrariesAsync()
        {
            return dbContext.Libraries.ToListAsync();
        }

        public Task AddAsync(Library library)
        {
            dbContext.Libraries.Add(library);
            return dbContext.SaveChangesAsync();
        }

        public Task DeleteByIdAsync(int id)
        {
            Library? item = dbContext.Libraries.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return Task.CompletedTask;
            dbContext.Libraries.Remove(item);
            return dbContext.SaveChangesAsync();
        }
    }
}