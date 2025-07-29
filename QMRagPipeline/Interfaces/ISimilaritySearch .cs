using QMRagPipeline.Models;

namespace QMRagPipeline.Interfaces
{
    public interface ISimilaritySearch
    {
        Task IndexAsync(List<EmbeddedChunk> chunks);
        Task<List<EmbeddedChunk>> SearchAsync(float[] queryEmbedding, int topK = 5);
    }
}
