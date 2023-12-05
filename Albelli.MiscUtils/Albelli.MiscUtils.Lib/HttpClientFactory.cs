
using System.Collections.Concurrent;
using System.Net.Http;
using BoDi;
using Microsoft.Extensions.Http;
using Polly;
//using Scenarios.Logging;
//using Serilog;

namespace Albelli.MiscUtils.Lib
{
    public class HttpClientFactory
    {
        private const string HttpClient = "HttpClient";
        private readonly static ConcurrentDictionary<string, HttpClient> _httpClients;

        static HttpClientFactory()
        {
            _httpClients = new ConcurrentDictionary<string, HttpClient>();
        }

        public static HttpClient GetHttpClient(IObjectContainer objectContainer)
        {
            return _httpClients.GetOrAdd(HttpClient, CreateHttpClient(objectContainer));
        }

        public static HttpClient GetHttpClient(IObjectContainer objectContainer, Uri baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl.ToString()))
            {
                // It is InvalidOperationException needed for SpecFlow because of ArgumentNullException is handled by SpecFlow
                throw new InvalidOperationException($"The parameter {baseUrl} is null");
            }

            return _httpClients.GetOrAdd(baseUrl.ToString(), CreateHttpClient(objectContainer, baseUrl));
        }

        private static HttpClient CreateHttpClient(IObjectContainer objectContainer, Uri baseUri)
        {
            var httpClient = CreateHttpClient(objectContainer);
            httpClient.BaseAddress = baseUri;
            return httpClient;
        }

        private static HttpClient CreateHttpClient(IObjectContainer objectContainer)
        {
            var httpClient = new HttpClient();

            return httpClient;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                                    retryAttempt)));
        }
    }
}
