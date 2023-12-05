using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;


namespace Albelli.MiscUtils.Lib
{
    public class RestClientFactory
    {
        public static T CreateUnauthorizedRestClient<T>(IObjectContainer objectContainer) where T : ServiceClient<T>
        {
            return CreateRestClient<T>(objectContainer, new UnauthorizedTokenProvider());
        }

        public static T CreateRestClient<T>(IObjectContainer objectContainer, ITokenProvider? tokenProvider = null) where T : ServiceClient<T>
        {
            var type = typeof(T);
            var configSectionName = type.Name;
            // AppClientV4 -> AppClient
            configSectionName = System.Text.RegularExpressions.Regex.Replace(configSectionName,
                @"V\d+$", "");
            var configuration = objectContainer.Resolve<IConfiguration>();
            var clientConfig = configuration.GetSection(configSectionName).Get<RestClientConfiguration>();
            return CreateRestClient<T>(objectContainer, clientConfig, tokenProvider);
        }

        public static T CreateRestClient<T>(IObjectContainer objectContainer, string scopes) where T : ServiceClient<T>
        {
            var type = typeof(T);
            var configSectionName = type.Name;
            var configuration = objectContainer.Resolve<IConfiguration>();
            var clientConfig = configuration.GetSection(configSectionName).Get<RestClientConfiguration>();
            clientConfig.Scopes = scopes;
            return CreateRestClient<T>(objectContainer, clientConfig);
        }

        public static T CreateRestClient<T>(IObjectContainer objectContainer,
            RestClientConfiguration clientConfig,
            ITokenProvider? tokenProvider = null) where T : ServiceClient<T>
        {
            T client;
            var type = typeof(T);
            var httpClient = HttpClientFactory.GetHttpClient(objectContainer);

            if (clientConfig == null || clientConfig.BaseUrl == null)
            {
                throw new NotSupportedException($"No valid configuration for for {type.Name}");
            }

            if (string.IsNullOrWhiteSpace(clientConfig.Scopes))
            {
                var freeAccessSignature = new[] { typeof(HttpClient), typeof(bool) };
                var freeAccessConstructor = type.GetConstructor(freeAccessSignature);
                if (freeAccessConstructor == null)
                {
                    throw new NotSupportedException($"No constructor without authentication for {type.FullName}");
                }

                client = (T)freeAccessConstructor.Invoke(new object[] { httpClient, false });
            }
            else
            {
                tokenProvider ??= TokenProviderFactory.GetTokenProvider(objectContainer, clientConfig.Scopes);
                var authenticatedSignature = new[] { typeof(ServiceClientCredentials), typeof(HttpClient), typeof(bool) };
                var authenticatedConstructor = type.GetConstructor(authenticatedSignature);
                if (authenticatedConstructor == null)
                {
                    throw new NotSupportedException($"No constructor with authentication for {type.FullName}");
                }

                client = (T)authenticatedConstructor.Invoke(new object[] { new TokenCredentials(tokenProvider), httpClient, false });
            }

            var baseUriProp = type.GetProperty("BaseUri");
            if (baseUriProp == null)
            {
                throw new NotSupportedException($"No BaseUri property on {type.FullName}");
            }

            baseUriProp.SetValue(client, clientConfig.BaseUrl);
            return client;
        }
    }
}