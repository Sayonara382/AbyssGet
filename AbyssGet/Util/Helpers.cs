using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AbyssGet.Crypto;
using AbyssGet.Tls;
using Jint;

namespace AbyssGet.Util;

public static class Helpers
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private static readonly JsonContext Context = new(Options);

    public static async Task<string> RequestPayload(string videoId)
    {
        var httpClient = new CustomHttpClient("abysscdn.com");
        var request = new HttpRequestMessage(HttpMethod.Get, videoId.StartsWith("http") ? videoId : $"https://abysscdn.com/?v={videoId}");
        
        request.Headers.ConnectionClose = true;
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        var response = await httpClient.SendAsync(request, TimeSpan.FromSeconds(30));

        var htmlCode = await response.Content.ReadAsStringAsync();

        var scriptRegex = new Regex("<script>(.*?)</script>");
        var scriptMatches = scriptRegex.Matches(htmlCode);
        
        var jsCode = scriptMatches.Select(m => m.Groups[1].Value).OrderByDescending(t => t.Length).First();

        var payloadRegex = new Regex(@"((\()?(?<name>\w+)\(0x\w+\)(\+)?){30,}");
        var payloadMatch = payloadRegex.Match(jsCode);
        
        var shiftRegex = new Regex($@"\(\){{(var {payloadMatch.Groups["name"]}=\w)");
        var shiftText = shiftRegex.Match(jsCode).Groups[1] + ";";

        var unusedFunctions = new List<string>
        {
            @"\w\(\w\(0x[0-9a-fA-F]+\),\(\)=>\{.*}\);",
            $@"function \w\(\)\{{var {payloadMatch.Groups["name"]}(.*?)decodeURIComponent(.*?)\}}\}}",
            @"var \w=\(function\(\)\{.*?\}\(\)\),(?<c>\w)=.*?;\}\);"
        };

        for (var i = 0; i < unusedFunctions.Count; i++)
        {
            var filterRegex = new Regex(unusedFunctions[i]);
            var functionCode = filterRegex.Match(jsCode);
            jsCode = jsCode.Replace(functionCode.Value, "");
            
            var functionCall = functionCode.Groups["c"].ToString();
            if (!string.IsNullOrEmpty(functionCall))
            {
                unusedFunctions.Add($@"{functionCall}\(\);");
            }
        }

        var engine = new Engine();
        
        engine.Execute("var window = {};");
        engine.Execute(jsCode);
        engine.Execute(shiftText);
        engine.Execute($"var payload = {payloadMatch};");
        
        return engine.GetValue("payload").AsString();
    }

    public static string DecodeCustomBase64(string input)
    {
        const string defaultCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
        const string customCharacterSet = "RB0fpH8ZEyVLkv7c2i6MAJ5u3IKFDxlS1NTsnGaqmXYdUrtzjwObCgQP94hoeW+/=";

        var builder = new StringBuilder(input.Length);

        foreach (var ch in input)
        {
            var index = customCharacterSet.IndexOf(ch);
            if (index != -1)
                builder.Append(defaultCharacterSet[index]);
        }

        var decodedBytes = Convert.FromBase64String(builder.ToString());
        return Encoding.UTF8.GetString(decodedBytes);
    }

    public static void MergeFiles(string baseDir, string tempDir, string fileName)
    {
        var downloadDir = Path.Combine(baseDir, tempDir);
        
        var files = Directory.GetFiles(downloadDir, "*.bin")
            .Select(f => new
            {
                Path = f,
                Number = int.TryParse(Path.GetFileNameWithoutExtension(f), out var n) ? n : int.MaxValue
            })
            .Where(f => f.Number != int.MaxValue)
            .OrderBy(f => f.Number)
            .Select(f => f.Path)
            .ToList();

        var outFile = Path.Combine(baseDir, fileName);
        using var output = File.Create(outFile);
        
        foreach (var file in files)
        {
            using var input = File.OpenRead(file);
            input.CopyTo(output);
        }
    }

    public static List<Video> ExtractVideos(string payload)
    {
        var decodedPayload = DecodeCustomBase64(payload);
        var jsonPayload = JsonSerializer.Deserialize(decodedPayload, Context.MetadataPayload)!;
   
        return Enumerable.Select<MetadataSource, Video>(jsonPayload.Sources, source => new Video
            {
                Domain = jsonPayload.Domain,
                Id = jsonPayload.Id,
                Md5Id = jsonPayload.Md5Id,
                Slug = jsonPayload.Slug,

                Codec = source.Codec,
                Label = source.Label,
                ResId = source.ResId,
                Size = source.Size,
                Type = source.Type
            })
            .OrderByDescending(video => int.Parse(video.Label![..^1]))
            .ToList();
    }
    
    public static List<List<Video>> ExtractVideos(IEnumerable<string> payloads) => payloads.Select(ExtractVideos).ToList();

    public static long GetEnd(long start, long totalSize)
    {
        if (totalSize < Abyss.StepSize)
            return totalSize - 1;

        var end = Math.Min(start + Abyss.StepSize, totalSize);
        return end - 1;
    }

    private static string GetRequestPayload(Video video, VideoRange range)
    {
        var videoPayload = new VideoCipherPayload
        {
            Slug = video.Slug!,
            Md5Id = video.Md5Id,
            Label = video.Label!,
            Size = video.Size,
            Range = range,
        };
        
        var videoJsonPayload = JsonSerializer.Serialize(videoPayload, Context.VideoCipherPayload);

        var cipher = AesCtr.FromMd5(video.Slug!);
        var payload = Encoding.UTF8.GetBytes(videoJsonPayload);
        cipher.EncryptDecrypt(payload);
        
        return JsonSerializer.Serialize(new VideoPayload
        {
            Hash = Encoding.Latin1.GetString(payload)
        }, Context.VideoPayload);
    }

    public static RequestPayload[] GeneratePayloads(Video video)
    {
        var totalSize = video.Size;
        var payloads = new RequestPayload[totalSize / Abyss.StepSize + 1];
        
        long start = 0;
        var i = 0;
        
        while (start < totalSize - 1)
        {
            var end = GetEnd(start, totalSize);
            var range = new VideoRange { Start = start, End = end};
            
            var message = GetRequestPayload(video, range);
            payloads[i] = new RequestPayload(message, start);
 
            start = end + 1;
            i++;
        }

        return payloads;
    }
}