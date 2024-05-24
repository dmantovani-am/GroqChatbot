using System.Text.Json.Nodes;

namespace GroqChatbot.Infrastructure.Groq;
public interface IGroqApiClient
{
    IAsyncEnumerable<JsonObject?> CreateChatCompletionStreamAsync(List<Message> messages);
}