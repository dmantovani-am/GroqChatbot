namespace GroqChatbot.Infrastructure.LLM;

public record Message(string Content, string Role = "user");