using System.Text.Json.Nodes;

namespace GroqChatbot.Infrastructure.LLM;

public interface IChatClient
{
    IAsyncEnumerable<JsonObject?> ChatComplete(IEnumerable<Message> messages, ChatCompleteParameters parameters);
}