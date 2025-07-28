namespace QMRagPipeline.Models
{
    public class ChatTurn
    {
        public AuthorRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public enum AuthorRole
    {
        System,
        User,
        Assistant
    }
}
