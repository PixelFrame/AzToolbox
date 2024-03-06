using AzToolbox.Models;
using System.Text.RegularExpressions;
using WinSdUtil.Lib;
using WinSdUtil.Lib.Model;

namespace AzToolbox.Services
{
    public class WinSdService
    {
        private WinSdConverter converter = null!;
        private readonly HttpClient _httpClient;
        public bool IsInitialized => converter != null;

        public WinSdService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InitializeAsync()
        {
            var trusteeJsonData = await _httpClient.GetStringAsync("/assets/WinSdTrustee.json");
            var adSchemaGuidJsonData = await _httpClient.GetStringAsync("/assets/WinSdAdSchemaGuid.json");

            converter = new WinSdConverter(true, trusteeJsonData, adSchemaGuidJsonData);
        }

        public AccessControlList FromSddlToAcl(string SDDL)
        {
            return converter.FromSddlToAcl(SDDL);
        }

        public AccessControlList FromBinaryToAcl(string BinaryStr)
        {
            var regDeNoise = new Regex(@"[\\\r\n\s,-]|0x");
            BinaryStr = regDeNoise.Replace(BinaryStr, "");
            var binary = Convert.FromHexString(BinaryStr);
            return converter.FromBinaryToAcl(binary);
        }
    }
}