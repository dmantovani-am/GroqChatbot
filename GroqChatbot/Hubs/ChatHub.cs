using GroqChatbot.Infrastructure.Groq;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Nodes;

namespace GroqChatbot.Hubs;

public class ChatHub(IGroqApiClient groqApiClient) : Hub
{
    const string assistant = "assistant";

    static readonly Message _system = new Message("You are a chatbot capable of anything and everything.", "system");

    readonly IGroqApiClient _groqApiClient = groqApiClient;

    public async Task ChatCompletion(string role, string message)
    {
        List<Message> messages =
        [
            _system,
            new(message),
        ];

        await foreach (JsonObject? chunk in _groqApiClient.CreateChatCompletionStreamAsync(messages))
        {
            string delta = chunk?["choices"]?[0]?["delta"]?["content"]?.ToString() ?? string.Empty;
            await Clients.Caller.SendAsync("ChatCompletionChunk", assistant, delta);
        }

        await Clients.Caller.SendAsync("ChatCompletionFinish");
    }
}
