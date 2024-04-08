using KzA.HEXEH.Base.FileAccess;

namespace AzToolbox.Services
{
    internal class HexehBlazorFileAccess(HttpClient httpClient) : IFileAccess
    {
        public bool UseAsync { get; set; } = true;

        public IEnumerable<DirectoryInfo> EnumExtensionDirs()
        {
            throw new NotSupportedException("Unable to perform sync file operation on Blazor WASM platform");
        }

        public Task<IEnumerable<DirectoryInfo>> EnumExtensionDirsAsync()
        {
            throw new NotSupportedException("Unable to support extension loading on Blazor WASM platform");
        }

        public IEnumerable<SchemaFile> EnumSchemas()
        {
            throw new NotSupportedException("Unable to perform sync file operation on Blazor WASM platform");
        }

        public async Task<IEnumerable<SchemaFile>> EnumSchemasAsync()
        {
            var result = new List<SchemaFile>();
            var schemaCsv = (await httpClient.GetStringAsync("/assets/HEXEH/schemas.csv")).Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var schema in schemaCsv)
            {
                var splits = schema.Split(',');
                var schemaFile = new SchemaFile()
                {
                    Name = splits[0],
                    RelativePath = splits[1].Trim(),        // The csv file will use CRLF not LF after pulled from git
                    Root = "/assets/HEXEH/"
                };
                result.Add(schemaFile);
            }
            return result;
        }

        public Task<Stream> GetSchemaReadStreamAsync(SchemaFile schema)
        {
            return httpClient.GetStreamAsync(schema.Root + schema.RelativePath);
        }

        public string ReadSchemaContent(SchemaFile schema)
        {
            throw new NotSupportedException("Unable to perform sync file operation on Blazor WASM platform");
        }
    }
}
