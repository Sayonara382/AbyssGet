using System.Globalization;
using System.Net;
using System.Text;

namespace AbyssGet.Tls;

public class CustomHttpClient(string host, int port = 443) : IDisposable
{
    private readonly TlsClient _tlsClient = new(host, port);
    
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestString = ToString(request);
        var responseString = await _tlsClient.SendRequestAsync(requestString, timeout);
        var response = ToResponseMessage(responseString);

        return response;
    }

    private static string ToString(HttpRequestMessage request)
    {
        using var stringWriter = new StringWriter();
        
        stringWriter.WriteLine($"{request.Method} {request.RequestUri!.PathAndQuery} HTTP/1.1");
        stringWriter.WriteLine($"Host: {request.RequestUri.Host}");

        foreach (var header in request.Headers) stringWriter.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");

        if (request.Content != null)
            foreach (var header in request.Content.Headers)
                stringWriter.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");

        stringWriter.WriteLine();

        if (request.Content != null) stringWriter.WriteLine(request.Content.ReadAsStringAsync().Result);

        return stringWriter.ToString();
    }

    private static HttpResponseMessage ToResponseMessage(string response)
    {
        var httpResponse = new HttpResponseMessage();

        using var stringReader = new StringReader(response);
        
        var statusLine = stringReader.ReadLine();
        if (statusLine != null)
        {
            var statusLineParts = statusLine.Split(' ');
            if (statusLineParts.Length >= 3)
                httpResponse.StatusCode = Enum.Parse<HttpStatusCode>(statusLineParts[1]);
        }

        while (stringReader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                break;

            var headerParts = line.Split([':'], 2);
            if (headerParts.Length == 2)
                httpResponse.Headers.TryAddWithoutValidation(headerParts[0].Trim(), headerParts[1].Trim());
        }

        var sb = new StringBuilder();
        
        while (stringReader.ReadLine() is var line && !string.IsNullOrEmpty(line) && int.Parse(line, NumberStyles.HexNumber) is var length && length > 0)
        {
            var buf = new char[length + 2];
            stringReader.Read(buf, 0, length + 2);
            sb.Append(buf[..^2]);
        }

        httpResponse.Content = new StringContent(sb.ToString());

        return httpResponse;
    }
    
    public void Dispose()
    {
        _tlsClient.Dispose();
    }
}