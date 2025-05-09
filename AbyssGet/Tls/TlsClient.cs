using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace AbyssGet.Tls;

public class TlsClient(string host, int port = 443) : IDisposable
{
    private readonly TcpClient _tcpClient = new();
    private SslStream? _sslStream;
    
    public async Task<string> SendRequestAsync(string request, TimeSpan timeout)
    {
        await _tcpClient.ConnectAsync(host, port);
            
        _sslStream = new SslStream(_tcpClient.GetStream(), false, (_, _, _, _) => true);

        var sslClientAuthenticationOptions = new SslClientAuthenticationOptions
        {
            TargetHost = host,
            ApplicationProtocols = [SslApplicationProtocol.Http11]
        };

        using var cts = new CancellationTokenSource(timeout);
            
        await _sslStream.AuthenticateAsClientAsync(sslClientAuthenticationOptions, cts.Token);

        var requestBytes = Encoding.UTF8.GetBytes(request);
        await _sslStream.WriteAsync(requestBytes, cts.Token);

        using var memoryStream = new MemoryStream();
        await _sslStream.CopyToAsync(memoryStream, cts.Token);
        var response = Encoding.UTF8.GetString(memoryStream.ToArray());

        return response;
    }
    
    public void Dispose()
    {
        _sslStream?.Dispose();
        _tcpClient.Dispose();
    }
}