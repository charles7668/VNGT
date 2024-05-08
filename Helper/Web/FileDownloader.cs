namespace Helper.Web
{
    public static class FileDownloader
    {
        private static readonly HttpClient _Client = new();

        /// <summary>
        /// download file , is success then message return file path
        /// </summary>
        /// <param name="fileUrl">file url</param>
        /// <param name="destinationPath">destination path</param>
        /// <returns><see cref="Tuple" /> with success and message</returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<(bool success, string message)> DownloadFileAsync(string fileUrl,
            string destinationPath)
        {
            try
            {
                using HttpResponseMessage response =
                    await _Client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                string fileName = response.Content.Headers.ContentDisposition?.FileName ?? Path.GetFileName(fileUrl);
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);
                string fileToWriteTo =
                    Path.Combine(
                        Path.GetFullPath(destinationPath) ??
                        throw new ArgumentException("destination is not valid"), fileName);
                await using Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create);
                await streamToReadFrom.CopyToAsync(streamToWriteTo);

                return (true, fileToWriteTo);
            }
            catch (Exception e)
            {
                return (false, $"Error downloading file: {e.Message}");
            }
        }
    }
}