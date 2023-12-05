using System.Collections.Concurrent;
using BoDi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;

namespace Albelli.MiscUtils.Lib
{
    public class TokenProviderFactory
    {
        private static readonly ConcurrentDictionary<string, ITokenProvider> _tokenProviders = new ConcurrentDictionary<string, ITokenProvider>();

        public static ITokenProvider GetTokenProvider(IObjectContainer objectContainer, string scopes)
        {
            var tokenProvider = _tokenProviders.GetOrAdd(scopes, s => CreateTokenProvider(objectContainer, s));
            return tokenProvider;
        }

        private static ITokenProvider CreateTokenProvider(IObjectContainer objectContainer, string scopes)
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var configuration = objectContainer.Resolve<IConfiguration>();
            var tokenEndpointUrl = configuration.GetSection("Authentication:TokenEndpointUrl").Value;
            var clientId = configuration.GetSection("Authentication:ClientId").Value;
            var clientSecret = configuration.GetSection("Authentication:ClientSecret").Value;
            //var logger = objectContainer.Resolve<ILogger>();
            return new TokenProvider(memoryCache, tokenEndpointUrl, clientId, clientSecret, scopes /*, logger*/);
        }
    }
}
