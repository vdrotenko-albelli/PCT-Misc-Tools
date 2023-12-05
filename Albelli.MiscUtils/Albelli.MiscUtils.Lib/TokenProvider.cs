using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Albelli.MiscUtils.Lib.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Http;
using Microsoft.Rest;
using Newtonsoft.Json;
using Polly;

namespace Albelli.MiscUtils.Lib
{
    public class TokenProvider : ITokenProvider
    {
        private const string GrantType = "client_credentials";
        private const string ApplicationJson = "application/json";

        private static readonly HttpClient _httpClient = CreateHttpClient();

        private readonly IMemoryCache _cache;
        private readonly string _connectTokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scopes;

        public TokenProvider(IMemoryCache cache, string connectTokenEndpoint, string clientId, string clientSecret, string scopes)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _connectTokenEndpoint = connectTokenEndpoint ?? throw new ArgumentNullException(nameof(connectTokenEndpoint));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            var accessToken = await _cache.GetOrCreateAsync(_clientId, async cacheEntry =>
            {
                var tokenResponse = await Policy.Handle<JsonReaderException>()
                    .WaitAndRetryAsync(PollyRetryDurations.ThreeTimesFast)
                    .ExecuteAsync(async () => await GetToken(cancellationToken, CreateRequestMessage()));
                var tokenExpiration = TimeSpan.FromSeconds(Convert.ToInt32(tokenResponse.ExpiresInSeconds ?? 300));
                cacheEntry.AbsoluteExpirationRelativeToNow = tokenExpiration.Subtract(TimeSpan.FromMinutes(5));
                return tokenResponse.Token;
            });
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private HttpRequestMessage CreateRequestMessage()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _connectTokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = GrantType,
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["scope"] = _scopes
                })
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
            return request;
        }

        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken, HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();
            TokenResponse tokenResponse;
            try
            {
                tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            }
            catch (Exception e)
            {
                //todo - replace with console //_logger.Error(e, $"Parsing token response failed, content is {responseContent}");
                throw;
            }

            if (!response.IsSuccessStatusCode || tokenResponse == null || !string.IsNullOrWhiteSpace(tokenResponse.Error))
            {
                //todo - replace with console //_logger.Error($"Token response did not contain required data, content is {responseContent}");
                throw new InvalidOperationException(
                    $"Access token retrieval failed with statuscode {response.StatusCode} and message {tokenResponse?.Error}.");
            }

            return tokenResponse;
        }

        private class TokenResponse
        {
            [JsonProperty("error")]
            public string Error { get; set; }
            [JsonProperty("expires_in")]
            public int? ExpiresInSeconds { get; set; }
            [JsonProperty("access_token")]
            public string Token { get; set; }
        }

        private static HttpClient CreateHttpClient()
        {
            var httpMessageHandler = new HttpClientHandler();
            var policyHandler = new PolicyHttpMessageHandler(HttpClientFactory.GetRetryPolicy()) { InnerHandler = httpMessageHandler };
            var httpClient = new HttpClient(policyHandler);
            return httpClient;
        }

    }
}