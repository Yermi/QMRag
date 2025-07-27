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
    }
}
