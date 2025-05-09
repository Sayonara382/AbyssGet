namespace AbyssGet.Util;

public class RequestPayload(string body, long start)
{
    public readonly string Body = body;
    public readonly long Start = start;
}

public class Settings
{
    public bool FirstUrlOnly { get; set; } = false;
    public int BestUrlPoolSize { get; set; } = 3;
    public int MaxThreads { get; set; } = 16;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(120);
    public TimeSpan BlockTimeout { get; set; } = TimeSpan.FromSeconds(60);
    public int RequestRetries { get; set; } = 3;
    public bool DownloadInParallel { get; set; } = false;
    public string OutputDirectory { get; set; } = ".";
}

public class Video
{
    // General
    public string? Domain { get; set; }
    public string? Id { get; set; }
    public int Md5Id { get; set; }
    public string? Slug { get; set; }
    
    // Video
    public string? Codec { get; set; }
    public string? Label { get; set; }
    public int ResId { get; set; }
    public long Size { get; set; }
    public string? Type { get; set; }
}