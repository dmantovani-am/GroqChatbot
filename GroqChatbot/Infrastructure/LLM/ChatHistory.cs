using System.Text.Json.Nodes;
using System.Text;

namespace GroqChatbot.Infrastructure.LLM;

public class ChatHistory
{
    readonly IChatClient _client;
    readonly List<Message> _history;

    public ChatHistory(IChatClient client, string? systemMessage = null)
    {
        _client = client ?? throw new ArgumentException(nameof(client));
        _history = new();

        if (systemMessage is not null) _history.Add(new(systemMessage, "system"));
    }

    public async IAsyncEnumerable<string> ChatComplete(string message, ChatCompleteParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(message);

        _history.Add(new(message));

        StringBuilder content = new();
        await foreach (JsonObject? chunk in _client.ChatComplete(_history, parameters))
        {
            string delta = chunk?["choices"]?[0]?["delta"]?["content"]?.ToString() ?? string.Empty;
            content.Append(delta);

            yield return delta;
        }

        _history.Add(new(content.ToString(), "assistant"));
    }

    public void Clear()
    {
        _history.Clear();
    }
}