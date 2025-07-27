using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;

namespace QMRagPipeline.Services
{
    public class DataLoader : IDataLoader
    {
        public Task<List<DocumentChunk>> LoadChunksAsync()
        {
            throw new NotImplementedException();
        }
    }
}
