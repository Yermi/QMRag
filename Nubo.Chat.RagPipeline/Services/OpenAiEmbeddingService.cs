using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Settings;

namespace QMRagPipeline.Services
{
    public class OpenAiEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _client;

        public OpenAiEmbeddingService(OpenAIClient client, IOptions<OpenAiSettings> settings)
        {
            var cfg = settings.Value;
            _client = client.GetEmbeddingClient(cfg.EmbeddingModel);
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var result = await _client.GenerateEmbeddingAsync(text);
            return result.Value.ToFloats().ToArray();
        }
    }
}
