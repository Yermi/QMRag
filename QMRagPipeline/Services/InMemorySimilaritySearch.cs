using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;

namespace QMRagPipeline.Services
{
    public class InMemorySimilaritySearch : ISimilaritySearch
    {
        private List<EmbeddedChunk> _index = new();

        public Task IndexAsync(List<EmbeddedChunk> chunks)
        {
            _index = chunks;
            return Task.CompletedTask;
        }


        public Task<List<EmbeddedChunk>> SearchAsync(float[] queryEmbedding, int topK = 5)
        {
            var results = _index
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList();

            return Task.FromResult(results);
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
