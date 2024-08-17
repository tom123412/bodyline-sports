using System.Text.Json.Serialization;

namespace bodyline_sports.Models;

public class Attachments
{
    public required AttachmentsData[] Data { get; set; }
}

public class AttachmentsData
{
    public required Media Media { get; set; }
    public SubAttachments? SubAttachments { get; set; }
}

public class Media
{
    public required Image Image { get; set; }
}

public class Image
{
    public required int Height { get; set;}
    public required int Width { get; set;}
    public required Uri Src { get; set;}
}

public class SubAttachments
{
    public required SubAttachmentsData[] Data { get; set; }
}

public class SubAttachmentsData
{
}