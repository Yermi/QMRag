using Microsoft.Extensions.Logging;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;

namespace QMRagPipeline.Services
{
    public class RagPipelineRunner
    {
        private readonly IDataLoader _dataLoader;
        private readonly IEmbeddingService _embeddingService;
        private readonly ISimilaritySearch _similaritySearch;
        private readonly IPromptComposer _promptComposer;
        private readonly ILlmService _llmService;
        private readonly ILogger<RagPipelineRunner> _logger;

        private List<EmbeddedChunk> _embeddedChunks = new();


        public RagPipelineRunner(
            IDataLoader dataLoader,
            IEmbeddingService embeddingService,
            ISimilaritySearch similaritySearch,
            IPromptComposer promptComposer,
            ILlmService llmService,
            ILogger<RagPipelineRunner> logger)
        {
            _dataLoader = dataLoader;
            _embeddingService = embeddingService;
            _similaritySearch = similaritySearch;
            _promptComposer = promptComposer;
            _llmService = llmService;
            _logger = logger;
        }


        public async Task BuildIndexAsync()
        {
            _logger.LogInformation("[INFO] Loading documents...");
    
            var chunks = await _dataLoader.LoadChunksAsync();

            _logger.LogInformation($"[INFO] Generating embeddings for {chunks.Count} chunks...");
    
            var embedded = new List<EmbeddedChunk>();

            int index = 1;
            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunk.Content);
                _logger.LogInformation($"[INFO] embeddings for chunk num: {index++} has created...");

                embedded.Add(new EmbeddedChunk
                {
                    Chunk = chunk,
                    Embedding = embedding
                });
            }

            _embeddedChunks = embedded;
            await _similaritySearch.IndexAsync(embedded);

            _logger.LogInformation("[INFO] Index built successfully.");
        }

        public async Task<string> AnswerQuestionAsync(string question)
        {
            var questionEmbedding = await _embeddingService.GetEmbeddingAsync(question);
            var topChunks = await _similaritySearch.SearchAsync(questionEmbedding, topK: 5);

            var prompt = _promptComposer.ComposeTurns(question, topChunks);
            var answer = await _llmService.GetAnswerAsync(prompt);

            return answer;
        }
    }
}
