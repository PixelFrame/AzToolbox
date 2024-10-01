using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace AzToolbox.Services
{
    public class CryptoService
    {
        private readonly IJSRuntime JsRuntime;

        public CryptoService(IJSRuntime JsRuntime)
        {
            this.JsRuntime = JsRuntime;
        }

        private static string HashPassphrase(string pass)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            return WebEncoders.Base64UrlEncode(bytes);
        }

        public async Task<string> Aes256Encrypt(string pass, string data)
        {
            return await Aes256Encrypt(pass, Encoding.UTF8.GetBytes(data));
        }

        public async Task<string> Aes256Encrypt(string pass, byte[] data)
        {
            var k = HashPassphrase(pass);
            var enc = await JsRuntime.InvokeAsync<byte[]>("AesEncrypt", [k, data]);
            return WebEncoders.Base64UrlEncode(enc);
        }
        public async Task<string> Aes256Decrypt(string pass, string data)
        {
            return await Aes256Decrypt(pass, WebEncoders.Base64UrlDecode(data));
        }

        public async Task<string> Aes256Decrypt(string pass, byte[] data)
        {
            var k = HashPassphrase(pass);
            var dec = await JsRuntime.InvokeAsync<byte[]>("AesDecrypt", [k, data]);
            return Encoding.UTF8.GetString(dec);
        }
    }
}
