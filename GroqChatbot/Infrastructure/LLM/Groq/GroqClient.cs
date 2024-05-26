using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace GroqChatbot.Infrastructure.LLM.Groq;

public class GroqClient : IChatClient
{
    private readonly HttpClient client = new();

    public GroqClient(string apiKey)
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    async IAsyncEnumerable<JsonObject?> CreateChatCompletionStreamInternalAsync(JsonObject request)
    {
        request.Add("stream", true);

        StringContent httpContent = new(request.ToJsonString(), Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

        HttpResponseMessage response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            String? line = await reader.ReadLineAsync();
            if (line is not null && line.StartsWith("data: "))
            {
                var data = line["data: ".Length..];
                if (data != "[DONE]")
                {
                    yield return JsonSerializer.Deserialize<JsonObject>(data);
                }
            }
        }
    }

    public async IAsyncEnumerable<JsonObject?> ChatComplete(IEnumerable<Message> messages, ChatCompleteParameters parameters)
    {
        JsonArray msgs = new();
        foreach (var message in messages)
        {
            msgs.Add(new JsonObject
            {
                ["role"] = message.Role,
                ["content"] = message.Content,
            });
        }

        JsonObject request = new()
        {
            ["model"] = parameters.Model,
            ["temperature"] = parameters.Temperature,
            ["max_tokens"] = parameters.MaxTokens,
            ["top_p"] = parameters.TopP,
            ["stop"] = parameters.Stop,
            ["messages"] = msgs,
        };

        await foreach (var r in CreateChatCompletionStreamInternalAsync(request)) yield return r;
    }
}