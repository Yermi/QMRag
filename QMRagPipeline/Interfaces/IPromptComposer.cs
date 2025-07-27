using QMRagPipeline.Models;

namespace QMRagPipeline.Interfaces
{
    public interface IPromptComposer
    {
        string ComposePrompt(string question, List<EmbeddedChunk> contextChunks);
    }
}
