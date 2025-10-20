namespace LearnLangs.Services.Chat
{
    public interface IGeminiChatService
    {
        Task<string> ReplyAsync(
            List<(string role, string content)> history,
            string userText,
            CancellationToken ct = default);
    }
}
