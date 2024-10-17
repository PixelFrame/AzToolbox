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
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return await AesEncrypt(pass, dataBytes, algo, length, iter);
        }

        public async Task<string> AesEncrypt(string pass, byte[] data, CryptoAlgorithm algo, int length, int iter)
        {
            var k = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            var algoName = algo.ToString().Replace('_', '-');
            var enc = await JsRuntime.InvokeAsync<byte[]>("AesEncrypt", [k, data, algoName, length, iter]);
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
