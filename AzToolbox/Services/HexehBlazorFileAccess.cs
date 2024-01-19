using KzA.HEXEH.Base.FileAccess;

namespace AzToolbox.Services
{
    internal class HexehBlazorFileAccess : IFileAccess
    {
        private readonly HttpClient _httpClient;

        public HexehBlazorFileAccess(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IEnumerable<DirectoryInfo> EnumExtensionDirs()
        {
            throw new NotSupportedException();
        }

        public IEnumerable<SchemaFile> EnumSchemas()
        {
            var result = new List<SchemaFile>();
            var schemaCsv = _httpClient.GetStringAsync("/assets/HEXEH/schemas.csv").Result.Split(Environment.NewLine);
            foreach (var schema in schemaCsv)
            {
                var splits = schema.Split(',');
                var schemaFile = new SchemaFile()
                {
                    Name = splits[0],
                    RelativePath = splits[1]
                };
                result.Add(schemaFile);
            }
            return result;
        }

        public string ReadSchemaContent(SchemaFile schema)
        {
            return _httpClient.GetStringAsync(schema.RelativePath).Result;
        }
    }
}
