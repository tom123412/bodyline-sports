using System.Text.Json.Serialization;

namespace bodyline_sports.Models;

public class Post
{
    public string? Message { get; set; }
    public Uri? PictureUrl { get; set; }
    public required string Id { get; set; } 
    
    //[JsonPropertyName("updated_time")]
    //public required DateTimeOffset UpdatedTime { get; set; }
}