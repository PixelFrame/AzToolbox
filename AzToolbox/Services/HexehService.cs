using KzA.HEXEH.Core.Parser;

namespace AzToolbox.Services
{
    public class HexehService(HttpClient httpClient)
    {
        public bool IsInitialized { get; private set; } = false;

        public async Task InitializeAsync()
        {
            if(IsInitialized) return;
            var fileAccess = new HexehBlazorFileAccess(httpClient);
            KzA.HEXEH.Core.Global.Configure(fileAccess);
            await KzA.HEXEH.Core.Global.InitializeAsync();
            IsInitialized = true;
        }
    }
}
