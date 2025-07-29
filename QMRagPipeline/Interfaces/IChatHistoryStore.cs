using QMRagPipeline.Models;

namespace QMRagPipeline.Interfaces
{
    public interface IChatHistoryStore
    {
        Task AppendMessageAsync(string sessionId, ChatTurn message);
        Task<List<ChatTurn>> GetHistoryAsync(string sessionId);
    }
}
