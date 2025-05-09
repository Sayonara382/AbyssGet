using System.Security.Cryptography;
using System.Text;

namespace AbyssGet.Crypto;

public class AesCtr
{
    private readonly byte[] _counter;
    private readonly Aes _aes;
    
    public AesCtr(byte[] key, byte[] counter)
    {
        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes");

        if (counter.Length != 16)
            throw new ArgumentException("Initial counter must be 16 bytes");
        
        _counter = (byte[])counter.Clone();
        
        _aes = Aes.Create();
        _aes.Mode = CipherMode.ECB;
        _aes.Padding = PaddingMode.None;
        _aes.Key = key;
    }

    public static AesCtr FromMd5(object input)
    {
        var md5Hash = Convert.ToHexStringLower(Md5.Hash(input));
        var hexHash = Encoding.UTF8.GetBytes(md5Hash);

        return new AesCtr(hexHash, hexHash[..16]);
    }
    
    public void EncryptDecrypt(byte[] data, int? amount = null)
    {
        var process = Math.Min(amount ?? data.Length, data.Length);
        var encryptedCounter = new byte[16];

        using var cryptoTransform = _aes.CreateEncryptor();
        for (var i = 0; i < process; i += 16)
        {
            cryptoTransform.TransformBlock(_counter, 0, 16, encryptedCounter, 0);

            var blockSize = Math.Min(16, process - i);
            for (var j = 0; j < blockSize; j++)
            {
                data[i + j] ^= encryptedCounter[j];
            }

            IncrementCounter(_counter);
        }
    }

    private static void IncrementCounter(byte[] counter)
    {
        for (var i = counter.Length - 1; i >= 0; i--)
        {
            if (++counter[i] != 0)
                break;
        }
    }
}