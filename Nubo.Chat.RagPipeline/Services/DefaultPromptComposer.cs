using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;
using System.Text;

namespace QMRagPipeline.Services
{
    public class DefaultPromptComposer : IPromptComposer
    {
        public string ComposePrompt(string question, List<EmbeddedChunk> contextChunks)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are a helpful assistant. Use the context below to answer the user's question.");
            sb.AppendLine();
            sb.AppendLine("### Context:");
            sb.AppendLine();

            foreach (var chunk in contextChunks)
            {
                sb.AppendLine($"[{chunk.Chunk.Source}]");
                sb.AppendLine(chunk.Chunk.Content);
                sb.AppendLine();
            }

            sb.AppendLine("### Question:");
            sb.AppendLine(question);
            sb.AppendLine();
            sb.AppendLine("### Answer:");

            return sb.ToString();
        }

        public List<ChatTurn> ComposeTurns(string userQuestion, List<EmbeddedChunk> contextChunks, List<ChatTurn>? chatHistory = null)
        {
            var result = new List<ChatTurn>();

            result.Add(new ChatTurn
            {
                Role = AuthorRole.System,
                Content = "You are a helpful assistant that answers only based on the given context. " +
                      "If the answer is not in the context, respond: 'This is not in context of my knowledge'"
            });

            foreach (var chunk in contextChunks)
            {
                if (!string.IsNullOrWhiteSpace(chunk.Chunk.Content))
                {
                    result.Add(new ChatTurn
                    {
                        Role = AuthorRole.Assistant,
                        Content = $"[context]: {chunk.Chunk.Content}"
                    });
                }
            }

            if (chatHistory != null)
                result.AddRange(chatHistory);

            result.Add(new ChatTurn
            {
                Role = AuthorRole.User,
                Content = userQuestion
            });

            return result;
        }
    }
}
