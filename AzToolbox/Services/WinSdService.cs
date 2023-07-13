using AzToolbox.Models;
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

        public TreeViewItem FromSddlToAcl(string SDDL, AccessMaskType maskType)
        {
            return converter.FromSddlToAcl(SDDL).ToTreeView(maskType);
        }
    }
}