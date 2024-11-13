using AzToolbox.Models;
using System.Text.RegularExpressions;
using WinSdUtil.Lib;
using WinSdUtil.Lib.Model;

namespace AzToolbox.Services
{
    public partial class WinSdService(HttpClient httpClient)
    {
        private WinSdConverter converter = null!;

        public bool IsInitialized => converter != null;

        public async Task InitializeAsync()
        {
            var trusteeJsonData = await httpClient.GetStringAsync("/assets/WinSdTrustee.json");
            var adGuidJsonData = await httpClient.GetStringAsync("/assets/WinSdAdGuid.json");

            converter = new WinSdConverter(true, trusteeJsonData, adGuidJsonData);
        }

        public AccessControlList FromSddlToAcl(string SDDL)
        {
            return converter.FromSddlToAcl(SDDL);
        }

        public AccessControlList FromBinaryToAcl(string BinaryStr)
        {
            var regDeNoise = RegDeNoise();
            BinaryStr = regDeNoise.Replace(BinaryStr, "");
            var binary = Convert.FromHexString(BinaryStr);
            return converter.FromBinaryToAcl(binary);
        }

        [GeneratedRegex(@"[\\\r\n\s,-]|0x")]
        private static partial Regex RegDeNoise();
    }
}