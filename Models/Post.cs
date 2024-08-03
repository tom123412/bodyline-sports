namespace bodyline_sports.Models;

public class Post
{
    public string? Message { get; set; }
    public Uri? PictureUrl { get; set; }
    public required string Id { get; set; } 
}