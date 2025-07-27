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

        private List<EmbeddedChunk> _embeddedChunks = new();


        public RagPipelineRunner(
            IDataLoader dataLoader,
            IEmbeddingService embeddingService,
            ISimilaritySearch similaritySearch,
            IPromptComposer promptComposer,
            ILlmService llmService)
        {
            _dataLoader = dataLoader;
            _embeddingService = embeddingService;
            _similaritySearch = similaritySearch;
            _promptComposer = promptComposer;
            _llmService = llmService;
        }


        public async Task BuildIndexAsync()
        {
            Console.WriteLine("[INFO] Loading documents...");
    
            var chunks = await _dataLoader.LoadChunksAsync();

            Console.WriteLine($"[INFO] Generating embeddings for {chunks.Count} chunks...");
    
            var embedded = new List<EmbeddedChunk>();

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunk.Content);
                embedded.Add(new EmbeddedChunk
                {
                    Chunk = chunk,
                    Embedding = embedding
                });
            }

            _embeddedChunks = embedded;
            _similaritySearch.Index(embedded);

            Console.WriteLine("[INFO] Index built successfully.");
        }

        public async Task<string> AnswerQuestionAsync(string question)
        {
            var questionEmbedding = await _embeddingService.GetEmbeddingAsync(question);
            var topChunks = _similaritySearch.Search(questionEmbedding, topK: 5);

            var prompt = _promptComposer.ComposePrompt(question, topChunks);
            var answer = await _llmService.GetAnswerAsync(prompt);

            return answer;
        }
    }
}
