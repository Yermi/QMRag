using QMRagPipeline.Models;

namespace QMRagPipeline.Interfaces
{
    public interface IDataLoader
    {
        Task<List<DocumentChunk>> LoadChunksAsync();
    }
}
