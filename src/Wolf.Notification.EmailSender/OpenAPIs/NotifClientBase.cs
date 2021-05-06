using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wolf.Notification.EmailSender.Config;

namespace Wolf.Notification.EmailSender.OpenAPIs
{
	public class NotifClientBase
    {
        private static DiscoveryDocumentResponse _authDiscoveryDocument = null;
        private static readonly MemoryCache _cache=new MemoryCache("AuthAccessToken");
        NotifApiOptions _notifApiOptions;
        public NotifClientBase(NotifApiOptions notifApiOptions)
		{
            _notifApiOptions = notifApiOptions;
        }

        protected async Task PrepareRequestAsync(HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
		{
            if (!string.IsNullOrEmpty(_notifApiOptions.UserAgentName)) request.Headers.Add("User-Agent", _notifApiOptions.UserAgentName);
            string accessToken = await GetAccessToken(client, _notifApiOptions.AuthOptions);
            request.SetBearerToken(accessToken);
        }

        protected async Task PrepareRequestAsync(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, StringBuilder urlBuilder) { 
            await Task.CompletedTask; 
        }

        protected async Task ProcessResponseAsync(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage responce, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets Authentication Discovery document either from cache or if not there yet from the well-known URL of STS server
        /// </summary>
        /// <param name="client"></param>
        /// <param name="authOptions"></param>
        /// <returns></returns>
        private async Task<DiscoveryDocumentResponse> GetAuthDescoveryDoc(HttpClient client, AuthenticationOptions authOptions)
		{
            if(null==_authDiscoveryDocument){
                var disco = await client.GetDiscoveryDocumentAsync(authOptions.StsUrl);
                if (disco.IsError) throw new ApplicationException($"Error getting discovery document from {authOptions.StsUrl}: {disco.Error}", disco.Exception);
                _authDiscoveryDocument = disco;
            }
            return _authDiscoveryDocument;
        }

        private async Task<string> GetAccessToken(HttpClient client, AuthenticationOptions authOptions)
		{
            if (null == authOptions) throw new ApplicationException("Authentiaction Options are not set");
            string cacheKey = $"{authOptions.ClientId}_{authOptions.Scope}";
            string accessToken = _cache.Get(cacheKey) as string;
            if(null== accessToken)
			{
                var disco = await GetAuthDescoveryDoc(client, authOptions);

                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,

                    ClientId = authOptions.ClientId,
                    ClientSecret = authOptions.ClientSecret,
                    Scope = authOptions.Scope
                });

                if (tokenResponse.IsError) throw new ApplicationException($"Error getting token from {disco.TokenEndpoint}: {tokenResponse.Error}", tokenResponse.Exception);

                accessToken=tokenResponse.AccessToken;
                int iCacheForSeconds = tokenResponse.ExpiresIn - 10;
                if (iCacheForSeconds > 0) {
                    _cache.Set(cacheKey, accessToken, DateTimeOffset.Now.AddSeconds(iCacheForSeconds));
                }
            }
            return accessToken;
        }
    }
}
