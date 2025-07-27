namespace QMRagPipeline.Models
{
    public class DocumentChunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}
