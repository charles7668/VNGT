using FileInfo = GameManager.Models.FileInfo;

namespace GameManager.Modules.Synchronizer.Drivers
{
    public interface IWebDAVDriver
    {
        void SetBaseUrl(string baseUrl);

        void SetAuthentication(string userName, string password);

        Task<List<FileInfo>> GetFilesAsync(string dirPath, int depth = 1);

        Task<List<string>> GetDirectories(string dirPath , int depth);

        Task Delete(string dirPath, CancellationToken cancellationToken);
        
        Task CreateFolderIfNotExistsAsync(string folderPath, CancellationToken cancellationToken);

        Task<byte[]> DownloadFileAsync(string filePath, CancellationToken cancellationToken);

        Task UploadFileAsync(string filePath, Stream fileStream, CancellationToken cancellationToken);
    }
}