using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace TIWebsite.Utils
{
    public static class EncodeUtils
    {
        public static string EncodeBase64Url(byte[] input)
            => Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        public static Guid EncodeUuidV3(string input)
        {
            var md5 = new MD5Digest();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            md5.BlockUpdate(inputBytes, 0, inputBytes.Length);
            byte[] hash = new byte[md5.GetDigestSize()];
            md5.DoFinal(hash, 0);

            hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

            return new Guid(
            [
                hash[3], hash[2], hash[1], hash[0],
            hash[5], hash[4],
            hash[7], hash[6],
            hash[8], hash[9], hash[10], hash[11],
            hash[12], hash[13], hash[14], hash[15]
            ]);
        }
    }
}
