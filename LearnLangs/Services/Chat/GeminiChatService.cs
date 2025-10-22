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

        private static string GuessApiVersion(string model, string? configured)
        {
            if (!string.IsNullOrWhiteSpace(configured)) return configured.Trim();
            var m = (model ?? "").ToLowerInvariant();
            if (m.StartsWith("gemini-1.") || m.StartsWith("gemini-1-") || m.StartsWith("gemini-1_")) return "v1";
            if (m.StartsWith("gemini-1.5") || m.StartsWith("gemini-1_5")) return "v1";
            return "v1beta"; // 2.x
        }

        public async Task<string> ReplyAsync(
            IEnumerable<(string role, string content)> history,
            string userText,
            string targetLanguage,
            string level,
            string goal,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
                throw new InvalidOperationException("GeminiAI:ApiKey is missing.");
            if (string.IsNullOrWhiteSpace(_opt.Model))
                throw new InvalidOperationException("GeminiAI:Model is missing.");

            var apiVer = GuessApiVersion(_opt.Model, _opt.ApiVersion);
            var baseUrl = string.IsNullOrWhiteSpace(_opt.Endpoint)
                ? "https://generativelanguage.googleapis.com"
                : _opt.Endpoint!.TrimEnd('/');
            var url = $"{baseUrl}/{apiVer}/models/{_opt.Model}:generateContent?key={_opt.ApiKey}";

            var systemPrompt =
$@"You are a professional, friendly multilingual language tutor.
Expertise: conversation coaching, pronunciation tips (IPA when helpful), grammar explained simply.
Adapt to learner level = {level}, target language = {targetLanguage}, goal = {goal}.
Rules:
- Reply concisely in **{targetLanguage}** unless asked otherwise.
- Encourage and ask natural follow-up questions.
- Gently correct mistakes; show brief examples and phonetic hints when useful.
- If learner uses another language, translate key phrases back to {targetLanguage} to reinforce learning.
- Be practical and context-aware.";

            var hist = (history ?? Enumerable.Empty<(string role, string content)>())
                       .Where(h => !string.IsNullOrWhiteSpace(h.content))
                       .ToList();
            if (hist.Count > 20) hist = hist.Skip(hist.Count - 20).ToList();

            var contents = new List<object>
            {
                new { role = "user", parts = new object[] { new { text = systemPrompt } } }
            };

            foreach (var (role, content) in hist)
            {
                var r = role?.ToLowerInvariant() == "assistant" ? "model" : "user";
                contents.Add(new { role = r, parts = new object[] { new { text = content } } });
            }

            contents.Add(new { role = "user", parts = new object[] { new { text = userText } } });

            var payload = new
            {
                contents,
                generationConfig = new
                {
                    temperature = 0.6,
                    topP = 0.9,
                    maxOutputTokens = 512
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini API error: {(int)resp.StatusCode} {resp.ReasonPhrase}. URL={url}\nBody: {body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.ValueKind != JsonValueKind.Array ||
                candidates.GetArrayLength() == 0)
                return "(no candidates)";

            foreach (var cand in candidates.EnumerateArray())
            {
                if (!cand.TryGetProperty("content", out var contentEl)) continue;
                if (!contentEl.TryGetProperty("parts", out var partsEl)) continue;
                if (partsEl.ValueKind != JsonValueKind.Array) continue;

                foreach (var p in partsEl.EnumerateArray())
                {
                    if (p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                    {
                        var text = t.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text!;
                    }
                }
            }

            return "(no content)";
        }
    }
}
