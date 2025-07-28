using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;
using QMRagPipeline.Settings;

namespace QMRagPipeline.Services
{


    public class QdrantRestSimilaritySearch : ISimilaritySearch
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantSettings _settings;

        public QdrantRestSimilaritySearch(HttpClient httpClient, IOptions<QdrantSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task IndexAsync(List<EmbeddedChunk> chunks)
        {
            var collectionExistsResponse = await _httpClient.GetAsync($"/collections/{_settings.CollectionName}");
            if (!collectionExistsResponse.IsSuccessStatusCode)
            {
                var createRequest = new
                {
                    vectors = new
                    {
                        size = 1536,
                        distance = "Cosine"
                    }
                };

                var createContent = new StringContent(
                    JsonSerializer.Serialize(createRequest),
                    Encoding.UTF8,
                    "application/json");

                var createResponse = await _httpClient.PutAsync($"/collections/{_settings.CollectionName}", createContent);
                createResponse.EnsureSuccessStatusCode();
            }

            var points = chunks.Select((chunk, index) => new
            {
                id = index,
                vector = chunk.Embedding,
                payload = new
                {
                    content = chunk.Chunk.Content,
                    source = chunk.Chunk.Source
                }
            });

            var upsertRequest = new
            {
                points = points.ToList()
            };

            var upsertContent = new StringContent(
                JsonSerializer.Serialize(upsertRequest),
                Encoding.UTF8,
                "application/json");

            var upsertResponse = await _httpClient.PutAsync($"/collections/{_settings.CollectionName}/points", upsertContent);
            upsertResponse.EnsureSuccessStatusCode();
        }

        public async Task<List<EmbeddedChunk>> SearchAsync(float[] queryEmbedding, int topK = 5)
        {
            var searchRequest = new
            {
                vector = queryEmbedding,
                top = topK,
                with_payload = true
            };

            var searchContent = new StringContent(
                JsonSerializer.Serialize(searchRequest),
                Encoding.UTF8,
                "application/json");

            var searchResponse = await _httpClient.PostAsync($"/collections/{_settings.CollectionName}/points/search", searchContent);
            searchResponse.EnsureSuccessStatusCode();

            using var stream = await searchResponse.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var resultList = new List<EmbeddedChunk>();
            if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                foreach (var item in resultElement.EnumerateArray())
                {
                    if (item.TryGetProperty("payload", out var payload))
                    {
                        var content = payload.GetProperty("content").GetString();
                        var source = payload.TryGetProperty("source", out var sourceVal) ? sourceVal.GetString() : null;

                        resultList.Add(new EmbeddedChunk
                        {
                            Chunk = new DocumentChunk
                            {
                                Content = content!,
                                Source = source
                            },
                            Embedding = Array.Empty<float>()
                        });
                    }
                }
            }

            return resultList;
        }

    }

}