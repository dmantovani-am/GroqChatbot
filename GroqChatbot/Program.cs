using GroqChatbot.Hubs;
using GroqChatbot.Infrastructure.LLM;
using GroqChatbot.Infrastructure.LLM.Groq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IChatClient>(_ =>
{
    var apiKey = builder.Configuration.GetValue<string>("GroqApiKey") ?? throw new Exception("Missing GroqApiKey");
    return new GroqClient(apiKey);
});

builder.Services.AddSingleton<ChatHistory>(s =>
{
    var client = s.GetRequiredService<IChatClient>();
    return new ChatHistory(client);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chat");

app.Run();
