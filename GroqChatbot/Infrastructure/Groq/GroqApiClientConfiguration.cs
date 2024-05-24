namespace GroqChatbot.Infrastructure.Groq;

public class GroqApiClientConfiguration
{
    public string Model { get; set; } = "llama3-70b-8192";

    public double Temperature { get; set; } = 0.5;

    public int MaxTokens { get; set; } = 4096;

    public int TopP { get; set; } = 1;

    public string Stop { get; set; } = "TERMINATE";
}