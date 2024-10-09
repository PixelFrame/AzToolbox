using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace AzToolbox.Services
{
    public class CryptoService(IJSRuntime JsRuntime)
    {
        private readonly IJSRuntime JsRuntime = JsRuntime;

        public async Task<string> Aes256Encrypt(string pass, string data, CryptoAlgorithm algo, int length)
        {
            var k = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            var algoName = algo.ToString().Replace('_', '-');
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var enc = await JsRuntime.InvokeAsync<byte[]>("AesEncrypt", [k, dataBytes, algoName, length]);
            return WebEncoders.Base64UrlEncode(enc);
        }

        public async Task<string> Aes256Decrypt(string pass, string data, CryptoAlgorithm algo, int length)
        {
            var k = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            var algoName = algo.ToString().Replace('_', '-');
            var dataBytes = WebEncoders.Base64UrlDecode(data);
            var dec = await JsRuntime.InvokeAsync<byte[]>("AesDecrypt", [k, dataBytes, algoName, length]);
            return Encoding.UTF8.GetString(dec);
        }
    }

    public enum CryptoAlgorithm
    {
        AES_CBC,
        AES_GCM,
        AES_CTR
    }
}
