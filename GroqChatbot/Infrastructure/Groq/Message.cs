namespace GroqChatbot.Infrastructure.Groq;

public record Message(string Content, string Role = "user");