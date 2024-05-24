using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace GroqChatbot.Infrastructure.Groq;

public class GroqApiClient : IGroqApiClient
{
    private readonly HttpClient client = new();
    private readonly GroqApiClientConfiguration _configuration;

    public GroqApiClient(string apiKey, GroqApiClientConfiguration? configuration = null)
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _configuration = configuration ?? new GroqApiClientConfiguration();
    }

    async Task<JsonObject?> CreateChatCompletionInternalAsync(JsonObject request)
    {
        // Commented out until stabilized on Groq
        // the API is still not accepting the request payload in its documented format, even after following the JSON mode instructions.
        // request.Add("response_format", new JsonObject(new KeyValuePair<string, JsonNode?>("type", "json_object")));

        StringContent httpContent = new(request.ToJsonString(), Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

        HttpResponseMessage response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();
        JsonObject? responseJson = JsonSerializer.Deserialize<JsonObject>(responseString);

        return responseJson;
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

    public async IAsyncEnumerable<JsonObject?> CreateChatCompletionStreamAsync(List<Message> messages)
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
            ["model"] = _configuration.Model,
            ["temperature"] = _configuration.Temperature,
            ["max_tokens"] = _configuration.MaxTokens,
            ["top_p"] = _configuration.TopP,
            ["stop"] = _configuration.Stop,
            ["messages"] = msgs,
        };

        await foreach (var r in CreateChatCompletionStreamInternalAsync(request)) yield return r;
    }
}