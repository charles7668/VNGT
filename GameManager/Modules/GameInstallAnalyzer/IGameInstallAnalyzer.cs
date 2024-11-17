using GameManager.Models;

namespace GameManager.Modules.GameInstallAnalyzer
{
    public interface IGameInstallAnalyzer
    {
        /// <summary>
        /// Analyze from file
        /// </summary>
        /// <param name="traceDataFilePath"></param>
        /// <param name="installFilePath"></param>
        /// <returns></returns>
        Task<Result<string?>> AnalyzeFromFileAsync(string traceDataFilePath, string installFilePath);
    }
}