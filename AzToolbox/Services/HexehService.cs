using KzA.HEXEH.Core.Parser;

namespace AzToolbox.Services
{
    public class HexehService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        public bool IsInitialized { get; private set; } = false;

        public void Initialize()
        {
            if(IsInitialized) return;
            var fileAccess = new HexehBlazorFileAccess(_httpClient);
            KzA.HEXEH.Core.Global.Configure(fileAccess);
            IsInitialized = true;
        }
    }
}
