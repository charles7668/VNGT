using GameManager.Services;
using Helper;
using Helper.Web;
using System.Net;
using System.Text;
using System.Xml;

namespace GameManager.Models.Synchronizer.Drivers
{
    public class WebDAVWebDAVDriver(IWebService webService) : IWebDAVDriver
    {
        private readonly HttpMethod _mkcolMethod = new("MKCOL");
        private readonly HttpMethod _profindMethod = new("PROPFIND");
        private string _baseUrl = "";
        private string _password = "";
        private string _userName = "";

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public void SetAuthentication(string userName, string password)
        {
            _userName = userName;
            _password = password;
        }

        public async Task<List<FileInfo>> GetFilesAsync(string dirPath,
            int depth = 1)
        {
            Dictionary<string, object> options = BuildBaseOptions();
            options["custom-headers"] = new Dictionary<string, string>
            {
                { "Depth", depth.ToString() }
            };
            var body = new StringContent("""
                                         <?xml version="1.0" encoding="UTF-8"?>
                                         <d:propfind xmlns:d="DAV:">
                                         	<d:prop xmlns:oc="https://owncloud.org/ns">
                                                 <d:getlastmodified />
                                                 <d:resourcetype />
                                                 <d:getcontentlength />
                                         	</d:prop>
                                         </d:propfind>
                                         """);
            HttpResponseMessage response = await Exec(_profindMethod, dirPath, body, options, CancellationToken.None);
            ThrowIfNotSuccess(response);

            string responseContent = await HttpContentHelper.DecompressContent(response.Content);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseContent);
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("D", "DAV:");
            XmlNodeList? responseNodes = xmlDoc.SelectNodes("//D:multistatus/D:response", nsmgr);
            if (responseNodes == null)
                return [];
            List<FileInfo> files = new();
            for (int i = 0; i < responseNodes.Count; ++i)
            {
                XmlNode? node = responseNodes[i];
                if (node == null)
                    continue;
                XmlNode? collectionNode = node.SelectSingleNode(".//D:resourcetype/D:collection", nsmgr);
                if (collectionNode != null)
                    continue;
                string? filePath = node.SelectSingleNode(".//D:href", nsmgr)?.InnerText;
                string? lastModified = node.SelectSingleNode(".//D:getlastmodified", nsmgr)?.InnerText;
                int size = int.Parse(node.SelectSingleNode(".//D:getcontentlength", nsmgr)?.InnerText ?? "-1");
                files.Add(new FileInfo
                {
                    FileName = filePath ?? "",
                    ModifiedTime = lastModified is null ? DateTime.MinValue : DateTime.Parse(lastModified),
                    Size = size
                });
            }

            return files;
        }

        public async Task<List<string>> GetDirectories(string dirPath, int depth)
        {
            Dictionary<string, object> options = BuildBaseOptions();
            options["custom-headers"] = new Dictionary<string, string>
            {
                { "Depth", depth.ToString() }
            };
            var body = new StringContent("""
                                         <?xml version="1.0" encoding="UTF-8"?>
                                         <d:propfind xmlns:d="DAV:">
                                         	<d:prop xmlns:oc="https://owncloud.org/ns">
                                                 <d:resourcetype />
                                         	</d:prop>
                                         </d:propfind>
                                         """);
            HttpResponseMessage response = await Exec(_profindMethod, dirPath, body, options, CancellationToken.None);
            ThrowIfNotSuccess(response);

            string responseContent = await HttpContentHelper.DecompressContent(response.Content);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseContent);
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("D", "DAV:");
            XmlNodeList? responseNodes = xmlDoc.SelectNodes("//D:multistatus/D:response", nsmgr);
            if (responseNodes == null)
                return [];
            List<string> dirs = new();
            for (int i = 0; i < responseNodes.Count; ++i)
            {
                XmlNode? node = responseNodes[i];
                if (node == null)
                    continue;
                XmlNode? collectionNode = node.SelectSingleNode(".//D:resourcetype/D:collection", nsmgr);
                if (collectionNode == null)
                    continue;
                string? dirHref = node.SelectSingleNode(".//D:href", nsmgr)?.InnerText;
                if (dirHref == null)
                    continue;
                dirs.Add(dirHref);
            }

            return dirs;
        }

        public async Task DeleteDirectory(string dirPath, CancellationToken cancellationToken)
        {
            Dictionary<string, object> options = BuildBaseOptions();
            HttpResponseMessage response = await Exec(HttpMethod.Delete, dirPath, new StringContent(""), options,
                cancellationToken);
            ThrowIfNotSuccess(response);
        }

        public async Task CreateFolderIfNotExistsAsync(string folderPath, CancellationToken cancellationToken)
        {
            folderPath = folderPath.TrimEnd('/') + "/";
            Dictionary<string, object> options = BuildBaseOptions();
            options["custom-headers"] = new Dictionary<string, string>
            {
                { "Depth", "0" }
            };
            var body = new StringContent("");
            HttpResponseMessage response = await Exec(_profindMethod, folderPath, body, options, cancellationToken);
            try
            {
                ThrowIfNotSuccess(response);
            }
            catch (FileNotFoundException)
            {
                response = await Exec(_mkcolMethod, folderPath, body, options, cancellationToken);
                ThrowIfNotSuccess(response);
            }
        }

        public async Task<byte[]> DownloadFileAsync(string filePath,
            CancellationToken cancellationToken)
        {
            Dictionary<string, object> options = BuildBaseOptions();
            HttpResponseMessage response =
                await Exec(HttpMethod.Get, filePath, new StringContent(""), options, cancellationToken);
            ThrowIfNotSuccess(response);
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        public async Task UploadFileAsync(string filePath, Stream fileStream,
            CancellationToken cancellationToken)
        {
            Dictionary<string, object> options = BuildBaseOptions();
            var body = new StreamContent(fileStream);
            HttpResponseMessage response = await Exec(HttpMethod.Put, filePath, body, options, cancellationToken);
            ThrowIfNotSuccess(response);
        }

        private Dictionary<string, object> BuildBaseOptions()
        {
            return new Dictionary<string, object>
            {
                { "webdav-url", _baseUrl },
                { "username", _userName },
                { "password", _password }
            };
        }

        private void ThrowIfNotSuccess(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException(),
                HttpStatusCode.NotFound => new FileNotFoundException(),
                _ => new Exception($"{response.Content.ReadAsStringAsync().Result}")
            };
        }

        private string BuildRequestUrl(string baseUrl, string path)
        {
            return baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
        }

        private async Task<HttpResponseMessage> Exec(HttpMethod method, string path, HttpContent body,
            Dictionary<string, object>? options, CancellationToken cancellationToken)
        {
            options ??= [];
            string webdavUrl = TryGetOption(options, "webdav-url", "");
            if (!webdavUrl.IsHttpLink())
                throw new ArgumentException("options[webdav-url] : webdav-url must be a valid http link");
            string userName = TryGetOption(options, "username", "");
            string password = TryGetOption(options, "password", "");

            HttpClient httpClient = webService.GetDefaultHttpClient();
            string requestUrl = BuildRequestUrl(webdavUrl, path);

            var httpRequest = new HttpRequestMessage(method, requestUrl)
            {
                Headers =
                {
                    { "Authorization", $"Basic {AuthToken(userName, password)}" }
                }
            };
            Dictionary<string, string> customHeaders =
                TryGetOption(options, "custom-headers", new Dictionary<string, string>());
            foreach ((string key, string value) in customHeaders)
            {
                httpRequest.Headers.TryAddWithoutValidation(key, value);
            }

            httpRequest.Content = body;
            return await httpClient.SendAsync(httpRequest, cancellationToken);
        }

        private T TryGetOption<T>(Dictionary<string, object> options, string key, T defaultValue)
        {
            if (options.TryGetValue(key, out object? o) && o is T t)
            {
                return t;
            }

            return defaultValue;
        }

        private string AuthToken(string username, string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        }
    }
}