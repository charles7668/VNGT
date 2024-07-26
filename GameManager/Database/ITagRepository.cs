using GameManager.DB.Models;

namespace GameManager.Database
{
    public interface ITagRepository
    {
        Task<Tag> AddTagAsync(string tag);

        Task<List<string>> GetAllTagsAsync();

        Task<Tag?> AnyAsync(string tagName);
    }
}