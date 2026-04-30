using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System.Text;
using System.Text.Json;

namespace TIWebsite.Pages;

public partial class Home
{
    private string _machineName = "";
    private string _output = "";
    private string _userName = "";


    private static string _fileName => "actdata.properties";

    private static string GenerateToken(string hwid)
    {
        string kid = "abc";
        int iat = 1700000000;
        int expiry = 2147483647;
        string dummyGuid = "00000000-0000-0000-0000-000000000000";

        string hwUuid = UuidV3FromString(hwid).ToString();

        string licenseTokenJson = $$"""
        {
          "sub": "{{dummyGuid}}",
          "TI-Nspire_CX_CAS_Student": true,
          "lic": "{{dummyGuid}}",
          "iss": "https://edtech.ti.com",
          "ibb": {{iat}},
          "exp": {{expiry}},
          "iat": {{iat}},
          "ibe": {{expiry}},
          "jti": "{{dummyGuid}}",
          "hw": "{{hwUuid}}",
          "rfr": {{expiry}}
        }
        """;

        var payloadDict = JsonSerializer.Deserialize<Dictionary<string, object>>(licenseTokenJson);

        var headerDict = new Dictionary<string, object>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT",
            ["kid"] = kid
        };
        string headerJson = JsonSerializer.Serialize(headerDict);
        string payloadJson = JsonSerializer.Serialize(payloadDict);

        string encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        string dataToSign = encodedHeader + "." + encodedPayload;
        byte[] dataToSignBytes = Encoding.UTF8.GetBytes(dataToSign);

        var keyGen = new RsaKeyPairGenerator();
        keyGen.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        var keyPair = keyGen.GenerateKeyPair();
        var privateKey = (RsaPrivateCrtKeyParameters)keyPair.Private;
        var publicKey = (RsaKeyParameters)keyPair.Public;

        var signer = new RsaDigestSigner(new Sha256Digest());
        signer.Init(true, privateKey);
        signer.BlockUpdate(dataToSignBytes, 0, dataToSignBytes.Length);
        byte[] signature = signer.GenerateSignature();

        string encodedSignature = Base64UrlEncode(signature);
        string jwt = dataToSign + "." + encodedSignature;

        byte[] modulusBytes = publicKey.Modulus.ToByteArray();
        if (modulusBytes[0] == 0)
        {
            byte[] trimmed = new byte[modulusBytes.Length - 1];
            Array.Copy(modulusBytes, 1, trimmed, 0, trimmed.Length);
            modulusBytes = trimmed;
        }
        string n = Base64UrlEncode(modulusBytes);

        string rawResponse =
            $"kty=RSA\ne=AQAB\nkid={kid}\nn={n}\n" +
            $"license_token={jwt}\n" +
            "access_token=aaaaaaaaaaaaaaaaaaaaaaaaaa\n" +
            "refresh_token=aaaaaaaaaaaaaaaaaaaaaaaaaa\n" +
            $"expires_in=25200\nissued_at={iat}\n" +
            "email=visit ocelot910 on GitHub";

        return Base64UrlEncode(Encoding.UTF8.GetBytes(rawResponse));
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static Guid UuidV3FromString(string input)
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