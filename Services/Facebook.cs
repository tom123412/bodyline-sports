using System.Text.Json.Serialization;
using bodyline_sports.Models;
using bodyline_sports.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace bodyline_sports.Services;

public interface IFacebook
{
    public Task<Group?> GetGroup(string groupdId);
    public Task<IEnumerable<Post>> GetGroupPosts(Group group);
    public Task<TokenDetails> GetTokenDetails(string accessToken, string userAccessToken);
    public Task<string> ExchangeForLongLivedToken(string accessToken);
}

public class Facebook : IFacebook
{
    private class AccessTokenDetails
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }

    private class GroupFeed
    {
        public required Post[] Data { get; set; }
    }

    private class Field
    {
        public required string Id { get; set; }

        [JsonPropertyName("object_id")]
        public string? ObjectId { get; set; }
    }

    private class Picture
    {
        public required PictureData Data { get; set; }
    }

    private class PictureData
    {
        public required Uri Url { get; set; }
    }

    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow =
            TimeSpan.FromHours(1)
    };
    private readonly HttpClient _httpClient;
    private readonly FacebookOptions _options;

    public Facebook(IHttpClientFactory httpClientFactory, IOptions<FacebookOptions> options, IMemoryCache cache)
    {
        _httpClient = httpClientFactory.CreateClient("Facebook");
        _options = options.Value;
        _cache = cache;
    }

    async Task<Group?> IFacebook.GetGroup(string groupId)
    {
        var cacheKey = $"Group-{groupId}";
        var url = $"{groupId}?fields=description,cover";
        var group = _cache.Get<Group>(cacheKey) ?? await _httpClient.GetFromJsonAsync<Group>(url);

        if (group is not null)
        {
            _cache.Set(cacheKey, group, _cacheOptions);
        }

        return group;
    }

    async Task<IEnumerable<Post>> IFacebook.GetGroupPosts(Group group)
    {
        var cacheKey = $"Posts-{group.Id}";
        var posts = _cache.Get<Post[]>(cacheKey)?.ToList() ?? [];
        var url = $"/{group.Id}/feed?fields=attachments,message,updated_time&since={posts.FirstOrDefault()?.UpdatedDateTime.ToString("s")}&limit={_options.PostsToLoad}";
        var feed = await _httpClient.GetFromJsonAsync<GroupFeed>(url);
        var newPosts = (feed?.Data ?? []).ToList();

        newPosts.AddRange(posts.Where(p => p.Type != "Status"));

        _cache.Set(cacheKey, newPosts.ToArray(), _cacheOptions);

        return _cache.Get<IEnumerable<Post>>(cacheKey)!;
    }

    async Task<TokenDetails> IFacebook.GetTokenDetails(string accessToken, string userAccessToken)
    {
        var url = $"/debug_token?input_token={accessToken}&access_token={userAccessToken}";
        var tokenDetails = await _httpClient.GetFromJsonAsync<TokenDetails>(url);
        return tokenDetails!;
    }

    async Task<string> IFacebook.ExchangeForLongLivedToken(string accessToken)
    {
        var url = $"/oauth/access_token?grant_type=fb_exchange_token&client_id={_options.AppId}&client_secret={_options.AppSecret}&fb_exchange_token={accessToken}";
        var longLivedToken = await _httpClient.GetFromJsonAsync<AccessTokenDetails>(url);
        return longLivedToken!.AccessToken;
    }
}