using SavePatcher.Models;

namespace SavePatcher.Patcher
{
    public interface IPatcher
    {
        /// <summary>
        /// Patch
        /// </summary>
        /// <returns></returns>
        Result Patch();

        /// <summary>
        /// Patch async
        /// </summary>
        /// <returns></returns>
        Task<Result> PatchAsync();
    }
}