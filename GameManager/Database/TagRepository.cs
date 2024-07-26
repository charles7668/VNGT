using GameManager.DB;
using GameManager.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GameManager.Database
{
    internal class TagRepository(AppDbContext context) : ITagRepository
    {
        public async Task<Tag> AddTagAsync(string tag)
        {
            Tag? existTag = await context.Tags
                .FirstOrDefaultAsync(x => x.Name == tag);
            if (existTag != null)
            {
                return existTag;
            }

            EntityEntry<Tag> result = await context.Tags.AddAsync(new Tag
            {
                Name = tag
            });
            return result.Entity;
        }

        public Task<List<string>> GetAllTagsAsync()
        {
            return context.Tags.Select(x => x.Name).ToListAsync();
        }
    }
}