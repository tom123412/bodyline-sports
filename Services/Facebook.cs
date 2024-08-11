using System.Diagnostics;
using System.Text.Json.Serialization;
using bodyline_sports.Models;
using bodyline_sports.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace bodyline_sports.Services;

public interface IFacebook
{
    public Task<Group?> GetGroup(string groupdId);  
    public Task<Post[]> GetGroupPosts(Group group);
}

public class Facebook : IFacebook
{
    private class GroupFeed
    {
        public required Post[] Data { get; set; }
    }

    private class Field
    {
        public required string Id { get; set;}
        
        [JsonPropertyName("object_id")]
        public string? ObjectId { get; set;}
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
    private readonly MemoryCacheEntryOptions options = new()
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
        var group = _cache.Get<Group>(cacheKey) ?? await _httpClient.GetFromJsonAsync<Group>($"/{groupId}?fields=description,cover&access_token={_options.AccessToken}");
        
        if (group is not null)
        {
            _cache.Set(cacheKey, group);
        }
        
        return group;
    }

    async Task<Post[]> IFacebook.GetGroupPosts(Group group)
    {
        var cacheKey = $"Posts-{group.Id}";
        var posts = _cache.Get<Post[]>(cacheKey)?.ToList() ?? [];
        var url = $"/{group.Id}/feed?since={posts.FirstOrDefault()?.UpdatedDateTime.ToString("s")}&limit=10&access_token={_options.AccessToken}";
        var feed = await _httpClient.GetFromJsonAsync<GroupFeed>(url);
        var newPosts = (feed?.Data ?? []).ToList();

        var tasks = new List<Task>();
        foreach (var post in newPosts)
        {
            tasks.Add(SetPictureInPost(post));
        }
        await Task.WhenAll(tasks);

        newPosts.AddRange(posts);

        _cache.Set(cacheKey, newPosts.ToArray());

        return _cache.Get<Post[]>(cacheKey)!;
    }

    private async Task SetPictureInPost(Post post)
    {
        var pictureId = (await _httpClient.GetFromJsonAsync<Field>($"/{post.Id}?fields=object_id&access_token={_options.AccessToken}"))?.ObjectId;
        if (pictureId is not null)
        {
            post.PictureUrl = (await _httpClient.GetFromJsonAsync<Picture>($"/{pictureId}/picture?redirect=false&access_token={_options.AccessToken}"))?.Data.Url;
        }
    }
}