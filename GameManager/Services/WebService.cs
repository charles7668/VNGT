namespace GameManager.Services
{
    public class WebService : IWebService
    {
        private static readonly HttpClient _HttpClient = new()
        {
            DefaultRequestHeaders =
            {
                { "Accept", "*/*" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:131.0) Gecko/20100101 Firefox/131.0" }
            }
        };

        public HttpClient GetDefaultHttpClient()
        {
            return _HttpClient;
        }
    }
}