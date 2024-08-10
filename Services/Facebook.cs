using System.Text.Json.Serialization;
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
        var feed = await _httpClient.GetFromJsonAsync<GroupFeed>($"/{group.Id}/feed?limit=10&access_token={_options.AccessToken}");
        var posts = feed?.Data ?? [];

        foreach (var post in posts)
        {
            await SetPictureInPost(post);
        }

        return posts;
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