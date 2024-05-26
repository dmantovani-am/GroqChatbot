namespace GroqChatbot.Infrastructure.LLM;

public class ChatCompleteParameters
{
    public string Model { get; set; } = "llama3-70b-8192";

    public double Temperature { get; set; } = 0.5;

    public int MaxTokens { get; set; } = 200;

    public int TopP { get; set; } = 1;

    public string Stop { get; set; } = "TERMINATE";
}