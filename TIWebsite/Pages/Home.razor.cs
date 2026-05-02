using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System.Text;
using System.Text.Json;
using static TIWebsite.Utils.EncodeUtils;

namespace TIWebsite.Pages;

public partial class Home
{
    // properties
    private string _machineName { set; get; } = "";
    private string _output { set; get; } = "";
    private string _userName { set; get; } = "";
    private string _userAgent { set; get; } = "";

    // constants
    private static string _fileName => "actdata.properties";
    private static string _kid => "abc";
    private static int _iat => 1700000000;
    private static int _expiry => 2147483647;
    private static string _guid => "00000000-0000-0000-0000-000000000000";

    private void GenerateLicense()
    {
        if (string.IsNullOrWhiteSpace(_machineName) || string.IsNullOrWhiteSpace(_userName))
        {
            _output = "Please enter a valid computer name or username.";
            return;
        }
        _output = GenerateToken(_machineName + _userName);
    }

    private static string GenerateToken(string hwid)
    {
        string hwUuid = EncodeUuidV3(hwid).ToString();

        string licenseTokenJson = $$"""
        {
          "sub": "{{_guid}}",
          "TI-Nspire_CX_CAS_Student": true,
          "lic": "{{_guid}}",
          "iss": "https://edtech.ti.com",
          "ibb": {{_iat}},
          "exp": {{_expiry}},
          "iat": {{_iat}},
          "ibe": {{_expiry}},
          "jti": "{{_guid}}",
          "hw": "{{hwUuid}}",
          "rfr": {{_expiry}}
        }
        """;

        var payloadDict = JsonSerializer.Deserialize<Dictionary<string, object>>(licenseTokenJson);

        var headerDict = new Dictionary<string, object>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT",
            ["kid"] = _kid
        };
        string headerJson = JsonSerializer.Serialize(headerDict);
        string payloadJson = JsonSerializer.Serialize(payloadDict);

        string encodedHeader = EncodeBase64Url(Encoding.UTF8.GetBytes(headerJson));
        string encodedPayload = EncodeBase64Url(Encoding.UTF8.GetBytes(payloadJson));
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

        string encodedSignature = EncodeBase64Url(signature);
        string jwt = dataToSign + "." + encodedSignature;

        byte[] modulusBytes = publicKey.Modulus.ToByteArray();
        if (modulusBytes[0] == 0)
        {
            byte[] trimmed = new byte[modulusBytes.Length - 1];
            Array.Copy(modulusBytes, 1, trimmed, 0, trimmed.Length);
            modulusBytes = trimmed;
        }
        string n = EncodeBase64Url(modulusBytes);

        string rawResponse =
            $"kty=RSA\ne=AQAB\nkid={_kid}\nn={n}\n" +
            $"license_token={jwt}\n" +
            "access_token=aaaaaaaaaaaaaaaaaaaaaaaaaa\n" +
            "refresh_token=aaaaaaaaaaaaaaaaaaaaaaaaaa\n" +
            $"expires_in=25200\nissued_at={_iat}\n" +
            "email=visit ocelot910 on GitHub";

        return EncodeBase64Url(Encoding.UTF8.GetBytes(rawResponse));
    }  
}