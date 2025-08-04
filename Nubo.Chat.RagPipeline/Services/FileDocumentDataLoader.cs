using DocumentFormat.OpenXml.Packaging;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;
using UglyToad.PdfPig;

namespace QMRagPipeline.Services
{
    public class FileDocumentDataLoader : IDataLoader
    {
        private readonly string _filePath;

        public FileDocumentDataLoader(string filePath)
        {
            _filePath = filePath;
        }

        public Task<List<DocumentChunk>> LoadChunksAsync()
        {
            var extension = Path.GetExtension(_filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => LoadPdfChunksAsync(),
                ".docx" => LoadWordChunksAsync(),
                ".txt" => LoadTxtChunksAsync(),
                ".json" => LoadJsonChunksAsync(),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}")
            };
        }

        private Task<List<DocumentChunk>> LoadPdfChunksAsync()
        {
            var chunks = new List<DocumentChunk>();
            using var document = PdfDocument.Open(_filePath);

            int position = 0;
            foreach (var page in document.GetPages())
            {
                var text = page.Text;
                var paragraphs = text.Split("\n\n");
                foreach (var paragraph in paragraphs)
                {
                    var content = paragraph.Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        chunks.Add(new DocumentChunk
                        {
                            Content = content,
                            Position = position++,
                            Source = Path.GetFileName(_filePath),
                            //SectionTitle = $"Page {page.Number}"
                        });
                    }
                }
            }

            return Task.FromResult(chunks);
        }

        private Task<List<DocumentChunk>> LoadWordChunksAsync()
        {
            var chunks = new List<DocumentChunk>();

            using var wordDoc = WordprocessingDocument.Open(_filePath, false);
            var body = wordDoc.MainDocumentPart?.Document.Body;

            if (body == null)
                return Task.FromResult(chunks);

            var paragraphs = body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();

            int position = 0;
            foreach (var para in paragraphs)
            {
                var text = para.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    chunks.Add(new DocumentChunk
                    {
                        Content = text,
                        Position = position++,
                        Source = Path.GetFileName(_filePath),
                        //SectionTitle = null
                    });
                }
            }

            return Task.FromResult(chunks);
        }

        private async Task<List<DocumentChunk>> LoadTxtChunksAsync()
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

        private async Task<List<DocumentChunk>> LoadJsonChunksAsync()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File not found: {_filePath}");

            var json = await File.ReadAllTextAsync(_filePath);

            var chunks = new List<DocumentChunk>();
            try
            {
                var array = System.Text.Json.JsonSerializer.Deserialize<List<object>>(json);
                if (array == null || array.Count == 0)
                    return chunks;

                int position = 0;
                for (int i = 0; i < array.Count; i += 10)
                {
                    var batch = array.Skip(i).Take(10);
                    var jsonString = System.Text.Json.JsonSerializer.Serialize(batch, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    chunks.Add(new DocumentChunk
                    {
                        Content = jsonString,
                        Source = Path.GetFileName(_filePath),
                        Position = position++
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse JSON file", ex);
            }

            return chunks;
        }


    }
}
