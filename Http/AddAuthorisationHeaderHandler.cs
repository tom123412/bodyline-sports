using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using bodyline_sports.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace bodyline_sports.Http;

public class AddAuthorisationHeaderHandler : DelegatingHandler
{

    private class FacebookOAuth
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FacebookOptions _options;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow =
            TimeSpan.FromHours(1)
    };

    public AddAuthorisationHeaderHandler(IHttpClientFactory httpClientFactory, IOptions<FacebookOptions> options, IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _cache = cache;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var hasAuthorizationHeader = request.Headers.Contains(HeaderNames.Authorization);
        var isOAuthUri = request.RequestUri?.AbsolutePath.Contains("access_token") ?? false;
        if (!(hasAuthorizationHeader || isOAuthUri))
        {
            //var cacheKey = "FacbookAccessToken";
            //var url = $"oauth/access_token?client_id={_options.AppId}&client_secret={_options.AppSecret}&grant_type=client_credentials";
            //var httpClient = _httpClientFactory.CreateClient("Facebook");
            //var accessToken = _cache.Get<string>(cacheKey) ?? (await httpClient.GetFromJsonAsync<FacebookOAuth>(url, cancellationToken))?.AccessToken;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", $"{_options.AccessToken}");
            //_cache.Set(cacheKey, accessToken, _cacheOptions);
        }
 
        return await base.SendAsync(request, cancellationToken);
    }
}