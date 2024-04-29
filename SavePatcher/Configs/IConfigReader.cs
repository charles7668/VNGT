using SavePatcher.Models;

namespace SavePatcher.Configs
{
    public interface IConfigReader<TResult>
    {
        /// <summary>
        /// read content to config object
        /// </summary>
        /// <param name="content">content</param>
        /// <returns></returns>
        Result<TResult> Read(string content);
    }
}