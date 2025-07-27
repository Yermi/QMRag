using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using QMRagPipeline.Interfaces;
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


        public async Task<string> GetAnswerAsync(string prompt)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that answers based only on the given context."),
                new UserChatMessage(prompt)
            };

            var response = await _client.CompleteChatAsync(messages);
            return response.Value?.Content?.FirstOrDefault()?.Text ?? "No answer generated.";
        }
    }
}
