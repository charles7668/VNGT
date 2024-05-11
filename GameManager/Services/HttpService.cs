namespace GameManager.Services
{
    public class HttpService : IHttpService
    {
        public HttpService()
        {
            DefaultClient = new HttpClient();
            DefaultClient.DefaultRequestHeaders.Add("User-Agent", "VNGT");
            DefaultClient.DefaultRequestHeaders.Add("Accept", "*/*");
            DefaultClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            DefaultClient.DefaultRequestHeaders.Add("Accept-Language", "*");
        }

        public HttpClient DefaultClient { get; }
    }
}