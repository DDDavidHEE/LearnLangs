using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LearnLangs.Services.Chat
{
    public interface IGeminiChatService
    {
        Task<string> ReplyAsync(
            IEnumerable<(string role, string content)> history,
            string userText,
            string targetLanguage,   // "English", "Vietnamese", ...
            string level,            // "beginner", "intermediate", "advanced"
            string goal,             // "conversation", "pronunciation", "grammar"
            CancellationToken ct = default);
    }
}
