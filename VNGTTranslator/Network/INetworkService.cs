using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VNGTTranslator.Network
{
    public interface INetworkService
    {
        HttpClient DefaultHttpClient { get; }

        Task<string> UnzipAsync(HttpContent content);
    }
}
