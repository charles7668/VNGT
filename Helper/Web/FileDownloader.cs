namespace Helper.Web
{
    public static class FileDownloader
    {
        private static readonly HttpClient _Client = new();

        public static async Task<(bool success, string errorMessage)> DownloadFileAsync(string fileUrl,
            string destinationPath)
        {
            try
            {
                using var response = await _Client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                await using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
                string fileToWriteTo = Path.GetFullPath(destinationPath);
                await using Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create);
                await streamToReadFrom.CopyToAsync(streamToWriteTo);

                return (true, string.Empty);
            }
            catch (Exception e)
            {
                return (false, $"Error downloading file: {e.Message}");
            }
        }
    }
}