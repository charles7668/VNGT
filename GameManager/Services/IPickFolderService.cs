using GameManager.Models;

namespace GameManager.Services
{
    public interface IPickFolderService
    {
        Task<Result> PickFolderAsync(Action<string> onSuccessCallback);
    }
}