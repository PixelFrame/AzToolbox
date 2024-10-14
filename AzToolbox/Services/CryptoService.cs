using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace AzToolbox.Services
{
    public class CryptoService(IJSRuntime JsRuntime)
    {
        private readonly IJSRuntime JsRuntime = JsRuntime;

        public async Task<string> AesEncrypt(string pass, string data, CryptoAlgorithm algo, int length, int iter)
        {
            var k = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            var algoName = algo.ToString().Replace('_', '-');
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var enc = await JsRuntime.InvokeAsync<byte[]>("AesEncrypt", [k, dataBytes, algoName, length, iter]);
            return WebEncoders.Base64UrlEncode(enc);
        }

        public async Task<string> AesDecrypt(string pass, string data, CryptoAlgorithm algo, int length, int iter)
        {
            var k = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            var algoName = algo.ToString().Replace('_', '-');
            var dataBytes = WebEncoders.Base64UrlDecode(data);
            var dec = await JsRuntime.InvokeAsync<byte[]>("AesDecrypt", [k, dataBytes, algoName, length, iter]);
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
