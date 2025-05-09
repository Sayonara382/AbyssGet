using System.Text;

namespace AbyssGet.Crypto;

public class Md5
{
    private static int[] ToPadded(byte[] byteData)
    {
        var bitLength = 8 * byteData.Length;
        var data = new int[16 + ((bitLength + 64) / 512 << 4)];

        for (int i = 0, step = 0; i < byteData.Length; i++, step += 8)
        {
            data[step / 32] |= byteData[i] << 24 - step % 32;
        }

        for (var i = 0; i < (byteData.Length - 1) * 8 / 32 + 1; i++)
        {
            data[i] = Endian(data[i]);
        }
        
        data[bitLength >>> 5] |= 128 << bitLength % 32;
        data[14 + (bitLength + 64 >>> 9 << 4)] = bitLength;

        return data;
    }

    private static byte[] WordsToBytes(long[] data)
    {
        var bytes = new byte[data.Length * 4];
        for (var i = 0; i < 32 * data.Length; i += 8)
        {
            bytes[i / 8] = (byte)(data[i >>> 5] >>> 24 - i % 32 & 255);
        }
        return bytes;
    }

    private static byte[] ConvertInputData(object input)
    {
        return input switch
        {
            string s => Encoding.ASCII.GetBytes(s),
            int or long => input.ToString()!.Select(b => (byte)(b - 48)).ToArray(),
            _ => throw new Exception("Invalid data type")
        };
    }

    private static long Ff(long n0, long n1, long n2, long n3, int num, int const0, int const1)
    {
        var tmp = (int)n0 + ((int)n1 & (int)n2 | ~(int)n1 & (int)n3) + num + const1;
        return (tmp << const0 | tmp >>> 32 - const0) + n1;
    }
    
    private static long Gg(long n0, long n1, long n2, long n3, int num, int const0, int const1)
    {
        var tmp = (int)n0 + ((int)n1 & (int)n3 | (int)n2 & ~(int)n3) + num + const1;
        return (tmp << const0 | tmp >>> 32 - const0) + n1;
    }
    
    private static long Hh(long n0, long n1, long n2, long n3, int num, int const0, int const1)
    {
        var tmp = (int)n0 + ((int)n1 ^ (int)n2 ^ (int)n3) + num + const1;
        return (tmp << const0 | tmp >>> 32 - const0) + n1;
    }
    
    private static long Ii(long n0, long n1, long n2, long n3, int num, int const0, int const1)
    {
        var tmp = (int)n0 + ((int)n2 ^ ((int)n1 | ~(int)n3)) + num + const1;
        return (tmp << const0 | tmp >>> 32 - const0) + n1;
    }

    private static int Endian(int value)
    {
        return (int)(0x00FF00FF & ((uint)value << 8 | (uint)value >> 24) | 0xFF00FF00u & ((uint)value << 24 | (uint)value >> 8));
    }

    private static long[] Endian(long[] array)
    {
        for (var i = 0; i < array.Length; i++)
            array[i] = Endian((int)array[i]);
        
        return array;
    }

    private static long[] Cycle(int[] data)
    {
        var start0 = 1732584193L;
        var start1 = -271733879L;
        var start2 = -1732584194L;
        var start3 = 271733878L;

        for (var i = 0; i < data.Length; i += 16)
        {
            var t0 = start0;
            var t1 = start1;
            var t2 = start2;
            var t3 = start3;
            
            start0 = Ff(start0, start1, start2, start3, data[i], 7, -680876936);
            start3 = Ff(start3, start0, start1, start2, data[i + 1], 12, -389564586);
            start2 = Ff(start2, start3, start0, start1, data[i + 2], 17, 606105819);
            start1 = Ff(start1, start2, start3, start0, data[i + 3], 22, -1044525330);
            start0 = Ff(start0, start1, start2, start3, data[i + 4], 7, -176418897);
            start3 = Ff(start3, start0, start1, start2, data[i + 5], 12, 1200080426);
            start2 = Ff(start2, start3, start0, start1, data[i + 6], 17, -1473231341);
            start1 = Ff(start1, start2, start3, start0, data[i + 7], 22, -45705983);
            start0 = Ff(start0, start1, start2, start3, data[i + 8], 7, 1770035416);
            start3 = Ff(start3, start0, start1, start2, data[i + 9], 12, -1958414417);
            start2 = Ff(start2, start3, start0, start1, data[i + 10], 17, -42063);
            start1 = Ff(start1, start2, start3, start0, data[i + 11], 22, -1990404162);
            start0 = Ff(start0, start1, start2, start3, data[i + 12], 7, 1804603682);
            start3 = Ff(start3, start0, start1, start2, data[i + 13], 12, -40341101);
            start2 = Ff(start2, start3, start0, start1, data[i + 14], 17, -1502002290);
            
            start0 = Gg(start0, start1 = Ff(start1, start2, start3, start0, data[i + 15], 22, 1236535329), start2, start3, data[i + 1], 5, -165796510);
            
            start3 = Gg(start3, start0, start1, start2, data[i + 6], 9, -1069501632);
            start2 = Gg(start2, start3, start0, start1, data[i + 11], 14, 643717713);
            start1 = Gg(start1, start2, start3, start0, data[i], 20, -373897302);
            start0 = Gg(start0, start1, start2, start3, data[i + 5], 5, -701558691);
            start3 = Gg(start3, start0, start1, start2, data[i + 10], 9, 38016083);
            start2 = Gg(start2, start3, start0, start1, data[i + 15], 14, -660478335);
            start1 = Gg(start1, start2, start3, start0, data[i + 4], 20, -405537848);
            start0 = Gg(start0, start1, start2, start3, data[i + 9], 5, 568446438);
            start3 = Gg(start3, start0, start1, start2, data[i + 14], 9, -1019803690);
            start2 = Gg(start2, start3, start0, start1, data[i + 3], 14, -187363961);
            start1 = Gg(start1, start2, start3, start0, data[i + 8], 20, 1163531501);
            start0 = Gg(start0, start1, start2, start3, data[i + 13], 5, -1444681467);
            start3 = Gg(start3, start0, start1, start2, data[i + 2], 9, -51403784);
            start2 = Gg(start2, start3, start0, start1, data[i + 7], 14, 1735328473);
            
            start0 = Hh(start0, start1 = Gg(start1, start2, start3, start0, data[i + 12], 20, -1926607734), start2, start3, data[i + 5], 4, -378558);

            start3 = Hh(start3, start0, start1, start2, data[i + 8], 11, -2022574463);
            start2 = Hh(start2, start3, start0, start1, data[i + 11], 16, 1839030562);
            start1 = Hh(start1, start2, start3, start0, data[i + 14], 23, -35309556);
            start0 = Hh(start0, start1, start2, start3, data[i + 1], 4, -1530992060);
            start3 = Hh(start3, start0, start1, start2, data[i + 4], 11, 1272893353);
            start2 = Hh(start2, start3, start0, start1, data[i + 7], 16, -155497632);
            start1 = Hh(start1, start2, start3, start0, data[i + 10], 23, -1094730640);
            start0 = Hh(start0, start1, start2, start3, data[i + 13], 4, 681279174);
            start3 = Hh(start3, start0, start1, start2, data[i], 11, -358537222);
            start2 = Hh(start2, start3, start0, start1, data[i + 3], 16, -722521979);
            start1 = Hh(start1, start2, start3, start0, data[i + 6], 23, 76029189);
            start0 = Hh(start0, start1, start2, start3, data[i + 9], 4, -640364487);
            start3 = Hh(start3, start0, start1, start2, data[i + 12], 11, -421815835);
            start2 = Hh(start2, start3, start0, start1, data[i + 15], 16, 530742520);
            
            start0 = Ii(start0, start1 = Hh(start1, start2, start3, start0, data[i + 2], 23, -995338651), start2, start3, data[i], 6, -198630844);

            start3 = Ii(start3, start0, start1, start2, data[i + 7], 10, 1126891415);
            start2 = Ii(start2, start3, start0, start1, data[i + 14], 15, -1416354905);
            start1 = Ii(start1, start2, start3, start0, data[i + 5], 21, -57434055);
            start0 = Ii(start0, start1, start2, start3, data[i + 12], 6, 1700485571);
            start3 = Ii(start3, start0, start1, start2, data[i + 3], 10, -1894986606);
            start2 = Ii(start2, start3, start0, start1, data[i + 10], 15, -1051523);
            start1 = Ii(start1, start2, start3, start0, data[i + 1], 21, -2054922799);
            start0 = Ii(start0, start1, start2, start3, data[i + 8], 6, 1873313359);
            start3 = Ii(start3, start0, start1, start2, data[i + 15], 10, -30611744);
            start2 = Ii(start2, start3, start0, start1, data[i + 6], 15, -1560198380);
            start1 = Ii(start1, start2, start3, start0, data[i + 13], 21, 1309151649);
            start0 = Ii(start0, start1, start2, start3, data[i + 4], 6, -145523070);
            start3 = Ii(start3, start0, start1, start2, data[i + 11], 10, -1120210379);
            start2 = Ii(start2, start3, start0, start1, data[i + 2], 15, 718787259);
            start1 = Ii(start1, start2, start3, start0, data[i + 9], 21, -343485551);
        
            start0 = (uint)((int)start0 + t0);
            start1 = (uint)((int)start1 + t1);
            start2 = (uint)((int)start2 + t2);
            start3 = (uint)((int)start3 + t3);
        }

        return Endian([start0, start1, start2, start3]);
    }

    public static byte[] Hash(object input)
    {
        var byteData = ConvertInputData(input);
        var data = ToPadded(byteData);
        
        var finalWords = Cycle(data);
        return WordsToBytes(finalWords);
    }
}