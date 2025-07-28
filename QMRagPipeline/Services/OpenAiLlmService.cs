using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;
using QMRagPipeline.Settings;

namespace QMRagPipeline.Services
{
    public class OpenAiLlmService : ILlmService
    {
        private readonly ChatClient _client;

        public OpenAiLlmService(OpenAIClient client, IOptions<OpenAiSettings> options)
        {
            var settings = options.Value;
            _client = client.GetChatClient(settings.ChatModel);
        }


        public async Task<string> GetAnswerAsync(List<ChatTurn> promptTurns)
        {
            var messages = MapToChatMessages(promptTurns);

            var response = await _client.CompleteChatAsync(messages);
            return response.Value?.Content?.FirstOrDefault()?.Text ?? "No answer generated.";
        }


        public List<ChatMessage> MapToChatMessages(List<ChatTurn> turns)
        {
            return turns
                .Select<ChatTurn, ChatMessage>(t => t.Role switch
                {
                    AuthorRole.User => new UserChatMessage(t.Content),
                    AuthorRole.Assistant => new AssistantChatMessage(t.Content),
                    AuthorRole.System => new SystemChatMessage(t.Content),
                    _ => throw new InvalidOperationException("Unknown role")
                })
                .ToList();
        }

    }
}
