namespace bodyline_sports.Options;

public sealed class FacebookOptions
{
    public required string GroupId { get; set; }
    public required string AccessToken { get; set; }
    public required string AppId { get; set; }
    public required string AppSecret { get; set; }
    public required string ConfigId { get; set; }
    public required int PostsToLoad { get; set; }
    public required string[] Administrators { get; set; }
    public required string DefaultAboutMessage { get; set;}
    public required Uri DefaultLogoUrl { get; set;}
}