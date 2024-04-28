using SavePatcher.Models;

namespace SavePatcher.Patcher
{
    public class SavePatcher : IPatcher
    {
        /// <summary>
        /// save file path , can use http or local file path
        /// </summary>
        private string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// if file is zip , password to extract zip
        /// </summary>
        public string ZipPassword { get; set; } = string.Empty;

        /// <summary>
        /// file list to specify which files in zip to patch
        /// </summary>
        public string[] PatchFiles { get; set; } = [];

        /// <summary>
        /// destination path , if the destination path is left empty, it will be selected manually
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <inheritdoc />
        public Result Patch()
        {
            throw new NotImplementedException();
        }

        public async Task<Result> PatchAsync()
        {
            if (FilePath.StartsWith("http://") || FilePath.StartsWith("https://"))
            {
            }
            else
            {
                string sourceFile = Path.GetFullPath(FilePath);
                string tempPath = Path.GetTempPath();
                string destFile = Path.Combine(tempPath);

                File.Copy(sourceFile, destFile, true);
            }

            return await Task.FromResult(Result.Ok());
        }
    }
}