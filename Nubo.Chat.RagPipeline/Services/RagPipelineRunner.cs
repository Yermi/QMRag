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
        private readonly IChatHistoryStore _chatHistoryStore;
        private readonly ILogger<RagPipelineRunner> _logger;

        public RagPipelineRunner(
            IDataLoader dataLoader,
            IEmbeddingService embeddingService,
            ISimilaritySearch similaritySearch,
            IPromptComposer promptComposer,
            ILlmService llmService,
            IChatHistoryStore chatHistoryStore,
            ILogger<RagPipelineRunner> logger)
        {
            _dataLoader = dataLoader;
            _embeddingService = embeddingService;
            _similaritySearch = similaritySearch;
            _promptComposer = promptComposer;
            _llmService = llmService;
            _chatHistoryStore = chatHistoryStore;
            _logger = logger;
        }


        public async Task BuildIndexAsync()
        {
            _logger.LogInformation("[RagPipelineRunner][BuildIndexAsync] Loading documents...");
    
            var chunks = await _dataLoader.LoadChunksAsync();

            _logger.LogInformation($"[RagPipelineRunner][BuildIndexAsync] Generating embeddings for {chunks.Count} chunks...");
    
            var embedded = new List<EmbeddedChunk>();

            int index = 1;
            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunk.Content);
                _logger.LogInformation($"[RagPipelineRunner][BuildIndexAsync] embeddings for chunk num: {index++} has created...");

                embedded.Add(new EmbeddedChunk
                {
                    Chunk = chunk,
                    Embedding = embedding
                });
            }

            await _similaritySearch.IndexAsync(embedded);

            _logger.LogInformation("[RagPipelineRunner][BuildIndexAsync] Index built successfully.");
        }

        public async Task<string> AnswerQuestionAsync(string question, string sessionId)
        {
            _logger.LogInformation("[RagPipelineRunner][AnswerQuestionAsync] get question.");

            var questionEmbedding = await _embeddingService.GetEmbeddingAsync(question);
            var topChunks = await _similaritySearch.SearchAsync(questionEmbedding, topK: 5);

            var chatHistory = await _chatHistoryStore.GetHistoryAsync(sessionId);
            var prompt = _promptComposer.ComposeTurns(question, topChunks, chatHistory);
            var answer = await _llmService.GetAnswerAsync(prompt);

            if (!string.IsNullOrEmpty(sessionId))
            {
                await _chatHistoryStore.AppendMessageAsync(sessionId, new ChatTurn { Content = question, Role = AuthorRole.User });
                await _chatHistoryStore.AppendMessageAsync(sessionId, new ChatTurn { Content = answer, Role = AuthorRole.Assistant });
            }

            _logger.LogInformation("[RagPipelineRunner][AnswerQuestionAsync] answer generated.");


            return answer;
        }
    }
}
