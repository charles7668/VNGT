using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace GameManager.Database
{
    internal class LibraryRepository(AppDbContext dbContext) : ILibraryRepository
    {
        public async Task<List<Library>> GetLibrariesAsync()
        {
            return await dbContext.Libraries.ToListAsync();
        }

        public async Task AddAsync(Library library)
        {
            await dbContext.Libraries.AddAsync(library);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            Library? item = await dbContext.Libraries.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return;
            dbContext.Libraries.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }
}