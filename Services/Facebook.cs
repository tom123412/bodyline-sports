using bodyline_sports.Models;
using bodyline_sports.Options;
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

    private readonly HttpClient _httpClient;
    private readonly FacebookOptions _options;

    public Facebook(IHttpClientFactory httpClientFactory, IOptions<FacebookOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient("Facebook");
        _options = options.Value;
    }

    async Task<Group?> IFacebook.GetGroup(string groupId)
    {
        var group = await _httpClient.GetFromJsonAsync<Group>($"/{groupId}?fields=description,cover&access_token={_options.AccessToken}");
        return group;
    }

    async Task<Post[]> IFacebook.GetGroupPosts(Group group)
    {
        var feed = await _httpClient.GetFromJsonAsync<GroupFeed>($"/{group.Id}/feed?access_token={_options.AccessToken}");
        return feed?.Data ?? [];
    }
}