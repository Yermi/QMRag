using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;

namespace QMRagPipeline.Services
{
    public class InMemorySimilaritySearch : ISimilaritySearch
    {
        private List<EmbeddedChunk> _index = new();

        public void Index(List<EmbeddedChunk> chunks)
        {
            _index = chunks;
        }

        public List<EmbeddedChunk> Search(float[] queryEmbedding, int topK = 5)
        {
            return _index
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList();
        }

        private float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0f;

            float dot = 0, normA = 0, normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-10));
        }

    }
}
