using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using bodyline_sports.Options;
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

    private readonly FacebookOptions _options;

    public AddAuthorisationHeaderHandler(IOptions<FacebookOptions> options)
    {
        _options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var hasAuthorizationHeader = request.Headers.Contains(HeaderNames.Authorization);
        var isOAuthUri = request.RequestUri?.AbsolutePath.Contains("access_token") ?? false;
        if (!(hasAuthorizationHeader || isOAuthUri))
        {
            var accessToken = _options.AccessToken;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", $"{accessToken}");
        }
 
        return await base.SendAsync(request, cancellationToken);
    }
}