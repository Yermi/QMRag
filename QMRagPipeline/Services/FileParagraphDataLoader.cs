using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;

namespace QMRagPipeline.Services
{
    public class FileParagraphDataLoader : IDataLoader
    {
        private readonly string _filePath;

        public FileParagraphDataLoader(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<DocumentChunk>> LoadChunksAsync()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File not found: {_filePath}");

            var text = await File.ReadAllTextAsync(_filePath);
            var paragraphs = text
                .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var chunks = new List<DocumentChunk>();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                chunks.Add(new DocumentChunk
                {
                    Content = paragraphs[i],
                    Source = Path.GetFileName(_filePath),
                    Position = i
                });
            }

            return chunks;
        }
    }
}
