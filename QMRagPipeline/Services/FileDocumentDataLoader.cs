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
    }
}
