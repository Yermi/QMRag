namespace QMRagPipeline.Settings
{
    public class QdrantSettings
    {
        public string Url { get; set; } = default!;
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 6333;
        public string ApiKey { get; set; } = default!;
        public string CollectionName { get; set; } = default!;
    }
}
