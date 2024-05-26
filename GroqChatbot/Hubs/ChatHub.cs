using GroqChatbot.Infrastructure.LLM;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Nodes;

namespace GroqChatbot.Hubs;

public class ChatHub(ChatHistory chatHistory) : Hub
{
    const string assistant = "assistant";

    static readonly Message _system = new Message("Sei un chatbot capace di tutto e di più.", "system");
    static readonly ChatCompleteParameters _parameters = new();

    readonly ChatHistory _chatHistory = chatHistory;

    public async Task ChatCompletion(string message, string model, double temperature, int maxTokens)
    {
        _parameters.Model = model;
        _parameters.Temperature = temperature;
        _parameters.MaxTokens = maxTokens;

        await foreach (var chunk in _chatHistory.ChatComplete(message, _parameters))
        {
            await Clients.Caller.SendAsync("ChatCompletionChunk", assistant, chunk);
        }

        await Clients.Caller.SendAsync("ChatCompletionFinish");
    }

    public void Clear()
    {
        _chatHistory.Clear();
    }
}
