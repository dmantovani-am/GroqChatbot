using GroqChatbot.Infrastructure.LLM;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace GroqChatbot.Hubs;

public class ChatHub(IChatClient chatClient, IDictionary<string, ChatHistory> chatHistoryMap) : Hub
{
    const string assistant = "assistant";

    static readonly Message _system = new Message("Sei un chatbot capace di tutto e di più.", "system");
    static readonly ChatCompleteParameters _parameters = new();
    
    readonly IChatClient _chatClient = chatClient;
    readonly IDictionary<string, ChatHistory> _chatHistoryMap = chatHistoryMap;

    public async Task ChatCompletion(string message, string model, double temperature, int maxTokens)
    {
        _parameters.Model = model;
        _parameters.Temperature = temperature;
        _parameters.MaxTokens = maxTokens;

        var connectionId = Context.ConnectionId;
        if (!_chatHistoryMap.TryGetValue(connectionId, out var chatHistory))         
        {
            chatHistory = new ChatHistory(_chatClient);
            _chatHistoryMap.Add(connectionId, chatHistory);
        }

        await foreach (var chunk in chatHistory.ChatComplete(message, _parameters))
        {
            await Clients.Caller.SendAsync("ChatCompletionChunk", assistant, chunk);
        }

        await Clients.Caller.SendAsync("ChatCompletionFinish");
    }

    public void Clear()
    {
        _chatHistoryMap.Clear();
    }
}
