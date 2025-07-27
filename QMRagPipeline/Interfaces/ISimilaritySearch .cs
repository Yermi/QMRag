using QMRagPipeline.Models;

namespace QMRagPipeline.Interfaces
{
    public interface ISimilaritySearch
    {
        void Index(List<EmbeddedChunk> chunks);
        List<EmbeddedChunk> Search(float[] queryEmbedding, int topK = 5);
    }
}
