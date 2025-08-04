using Microsoft.Extensions.Logging;
using QMRagPipeline.Interfaces;
using QMRagPipeline.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace QMRagPipeline.Services
{
    public class ChatHistoryStore : IChatHistoryStore
    {
        private readonly IDatabase _db;
        private readonly ILogger<ChatHistoryStore> _logger;

        public ChatHistoryStore(IConnectionMultiplexer redis, ILogger<ChatHistoryStore> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task AppendMessageAsync(string sessionId, ChatTurn message)
        {
            var key = $"chat:session:{sessionId}";
            var json = JsonSerializer.Serialize(message);
            await _db.ListRightPushAsync(key, json);
            await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(60));
        }

        public async Task<List<ChatTurn>> GetHistoryAsync(string sessionId)
        {
            var key = $"chat:session:{sessionId}";
            var entries = await _db.ListRangeAsync(key);
            var list = new List<ChatTurn>();
            foreach (var entry in entries)
            {
                if (!entry.IsNullOrEmpty)
                {
                    try
                    {
                        var turn = JsonSerializer.Deserialize<ChatTurn>(entry!);
                        if (turn != null)
                            list.Add(turn);
                    }
                    catch (Exception ex)
                    {
                        {
                            _logger.LogError($"[ChatHistoryStore][GetHistoryAsync] error serialzing message: {ex.Message}");
                        }
                    }
                }
            }

            return list;
        }
    }
}
