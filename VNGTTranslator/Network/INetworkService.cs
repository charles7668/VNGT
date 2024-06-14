using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VNGTTranslator.Models;

namespace VNGTTranslator.Network
{
    public interface INetworkService
    {
        HttpClient DefaultHttpClient { get; }

        HttpClient? ProxyHttpClient { get; }

        Task<string> UnzipAsync(HttpContent content);

        void UpdateProxySettingUsingAppConfig();
    }
}
