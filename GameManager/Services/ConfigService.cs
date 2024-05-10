using GameManager.Attributes;
using System.Reflection;

namespace GameManager.Services
{
    /// <summary>
    /// the config service for debug
    /// </summary>
    public class ConfigService : IConfigService
    {
        [NeedCreate]
        private string CoverFolder => Path.Combine(ConfigFolder, "covers");

        [NeedCreate]
        public string ConfigFolder { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs");

        public void CreateConfigFolderIfNotExistAsync()
        {
            PropertyInfo[] properties =
                GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttribute<NeedCreateAttribute>() == null)
                    continue;
                object? obj = propertyInfo.GetValue(this);
                if (obj is not string dir)
                {
                    continue;
                }

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        public async Task<string> AddCoverImage(string srcFile)
        {
            // generate random file name
            string id = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(srcFile);
            await ReplaceCoverImage(srcFile, id + extension);
            return id + extension;
        }

        public async Task ReplaceCoverImage(string? srcFile, string? coverName)
        {
            if (coverName == null || srcFile == null)
                throw new ArgumentException("path is null");
            if (!File.Exists(srcFile))
                throw new ArgumentException("file is not exist");
            string? coverPath = await GetCoverFullPath(coverName);
            if (coverPath == null)
                return;
            File.Copy(srcFile, coverPath, true);
        }

        public Task<string?> GetCoverFullPath(string? coverName)
        {
            if (coverName == null)
                return Task.FromResult<string?>(null);
            string fullPath = Path.Combine(CoverFolder, coverName);
            return Task.FromResult(fullPath)!;
        }

        public async Task DeleteCoverImage(string? coverName)
        {
            if (coverName == null)
                return;
            string? fullPath = await GetCoverFullPath(coverName);
            if (fullPath == null || !File.Exists(fullPath))
                return;
            File.Delete(Path.Combine(CoverFolder, coverName));
        }
    }
}