using System.Text.Json.Serialization;

namespace AbyssGet.Util;

public class MetadataSource
{
    public required string Codec { get; set; }
    public required string Label { get; set; }
    public required int ResId { get; set; }
    public required long Size { get; set; }
    public required string Type { get; set; }
}

public class MetadataPayload
{
    public required string Domain { get; set; }
    public required string Id { get; set; }
    public required int Md5Id { get; set; }
    public required string Slug { get; set; }
    public required List<MetadataSource> Sources { get; set; }
}

public class VideoRange
{
    public required long Start { get; set; }
    public required long End { get; set; }
}

public class VideoCipherPayload
{
    public required string Slug { get; set; }
    public required int Md5Id { get; set; }
    public required string Label { get; set; }
    public required long Size { get; set; }
    public required VideoRange Range { get; set; }
}

public class VideoPayload
{
    public required string Hash { get; set; }
}

[JsonSerializable(typeof(MetadataPayload))]
[JsonSerializable(typeof(MetadataSource))]
[JsonSerializable(typeof(VideoCipherPayload))]
[JsonSerializable(typeof(VideoPayload))]
[JsonSerializable(typeof(List<string>))]
public partial class JsonContext : JsonSerializerContext;