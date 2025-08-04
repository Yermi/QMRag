namespace QMRagPipeline.Models
{
    public class EmbeddedChunk
    {
        public DocumentChunk Chunk { get; set; } = default!;
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
