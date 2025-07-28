using QMRagPipeline.Models;

namespace QMRagPipeline.Interfaces
{
    public interface ILlmService
    {
        Task<string> GetAnswerAsync(List<ChatTurn> promptTurns);
    }
}
