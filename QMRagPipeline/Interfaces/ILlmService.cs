namespace QMRagPipeline.Interfaces
{
    public interface ILlmService
    {
        Task<string> GetAnswerAsync(string prompt);
    }
}
