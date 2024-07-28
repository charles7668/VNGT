using GameManager.Models;

namespace GameManager.Extractor
{
    public interface IExtractor
    {
        /// <summary>
        /// Support extensions
        /// </summary>
        string[] SupportExtensions { get; }

        /// <summary>
        /// Extract file
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="option">option</param>
        /// <returns>extracted file path</returns>
        public Task<Result<string>> ExtractAsync(string filePath, ExtractOption option);
    }
}