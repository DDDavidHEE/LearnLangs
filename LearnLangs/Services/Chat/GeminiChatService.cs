using System.Net.Http;
using System.Text;
using System.Text.Json;
using LearnLangs.Options;
using Microsoft.Extensions.Options;

namespace LearnLangs.Services.Chat
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _http;
        private readonly GeminiAiOptions _opt;

        public GeminiChatService(HttpClient http, IOptions<GeminiAiOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task<string> ReplyAsync(
            List<(string role, string content)> history,
            string userText,
            CancellationToken ct = default)
        {
            // prompt đơn giản: (có thể cải tiến system prompt tại đây)
            var systemPrompt = "You are a helpful English tutor. Reply concisely.";

            // build parts: system + last 6 turns + user
            var parts = new List<object> { new { text = systemPrompt } };

            // lấy ~6 trao đổi gần nhất để gọn
            foreach (var (role, content) in history.TakeLast(12))
            {
                parts.Add(new { text = $"{role.ToUpper()}: {content}" });
            }

            parts.Add(new { text = $"USER: {userText}" });

            var url = $"{_opt.Endpoint.TrimEnd('/')}/{_opt.ApiVersion}/models/{_opt.Model}:generateContent?key={_opt.ApiKey}";

            var payload = new
            {
                contents = new object[] { new { parts } },
                generationConfig = new { temperature = 0.6 }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            try
            {
                var resp = await _http.SendAsync(req, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"Gemini {resp.StatusCode}: {body}");

                using var doc = JsonDocument.Parse(body);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content").GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                return string.IsNullOrWhiteSpace(text) ? "(no content)" : text.Trim();
            }
            catch (Exception ex)
            {
                // fallback để kiểm tra pipeline, tránh “im lặng”
                return $"(echo) {userText}\n\n[Gemini error: {ex.Message}]";
            }
        }
    }
}
