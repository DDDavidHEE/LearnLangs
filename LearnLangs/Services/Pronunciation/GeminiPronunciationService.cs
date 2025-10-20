using System.Net.Http;
using System.Text;
using System.Text.Json;
using LearnLangs.Options;
using Microsoft.Extensions.Options;

namespace LearnLangs.Services.Pronunciation
{
    public class GeminiPronunciationService : IGeminiPronunciationService
    {
        private readonly HttpClient _http;
        private readonly GeminiAiOptions _opts;

        public GeminiPronunciationService(HttpClient http, IOptions<GeminiAiOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
        }

        public async Task<PronunciationResultDto> AssessWithGeminiAsync(
     byte[] audioBytes, string referenceText, string mimeType = "audio/webm", CancellationToken ct = default)
        {
            var baseUrl = (_opts.Endpoint ?? "https://generativelanguage.googleapis.com").TrimEnd('/');
            var apiVer = string.IsNullOrWhiteSpace(_opts.ApiVersion) ? "v1beta" : _opts.ApiVersion.Trim('/');
            var model = _opts.Model ?? "gemini-2.5-flash";

            var url = $"{baseUrl}/{apiVer}/models/{model}:generateContent?key={_opts.ApiKey}";
            var base64 = Convert.ToBase64String(audioBytes);

            var payload = new
            {
                contents = new object[]
                {
            new
            {
                parts = new object[]
                {
                    new { text =
                        $"Analyze the learner's pronunciation versus this reference: \"{referenceText}\". " +
                        "Return STRICT JSON with fields: " +
                        "{ \"score\": number, \"accuracy\": number, \"fluency\": number, " +
                        "\"prosody\": number, \"completeness\": number, " +
                        "\"word_details\": [ { \"word\": string, \"error\": string, \"accuracy\": number } ] }." },
                    new
                    {
                        inline_data = new
                        {
                            mime_type = mimeType,
                            data = base64
                        }
                    }
                }
            }
                },
                // 👇 Force real JSON response
                generationConfig = new { temperature = 0.2, response_mime_type = "application/json" }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Gemini API error: {(int)resp.StatusCode} {resp.ReasonPhrase}. URL={url}\nBody: {body}");
            }

            // --- Extract the JSON safely ---
            using var doc = JsonDocument.Parse(body);
            var parts = doc.RootElement.GetProperty("candidates")[0]
                             .GetProperty("content").GetProperty("parts");

            // Try to find a part with JSON text
            string? jsonStr = null;
            foreach (var p in parts.EnumerateArray())
            {
                if (p.TryGetProperty("text", out var t))
                {
                    var raw = t.GetString();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        jsonStr = CleanToJson(raw!);
                        if (jsonStr != null) break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(jsonStr))
                throw new InvalidOperationException("Gemini returned no JSON text to parse.");

            using var parsed = JsonDocument.Parse(jsonStr);
            var r = parsed.RootElement;

            double score = r.TryGetProperty("score", out var ps) ? ps.GetDouble() : 0;
            double accuracy = r.TryGetProperty("accuracy", out var pa) ? pa.GetDouble() : score * 0.8;
            double fluency = r.TryGetProperty("fluency", out var pf) ? pf.GetDouble() : score * 0.7;
            double prosody = r.TryGetProperty("prosody", out var pp) ? pp.GetDouble() : score * 0.6;
            double completeness = r.TryGetProperty("completeness", out var pc) ? pc.GetDouble() : score * 0.9;

            var words = new List<WordScoreDto>();
            if (r.TryGetProperty("word_details", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in arr.EnumerateArray())
                {
                    words.Add(new WordScoreDto(
                        el.TryGetProperty("word", out var w) ? w.GetString() ?? "" : "",
                        el.TryGetProperty("error", out var e) ? e.GetString() ?? "" : "",
                        el.TryGetProperty("accuracy", out var a) ? a.GetDouble() : 0
                    ));
                }
            }

            return new PronunciationResultDto(
                score, accuracy, fluency, completeness, prosody, words, body
            );
        }

        // Helper: strip ```json fences or extract the first {...} block
        private static string? CleanToJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            raw = raw.Trim();

            // Remove markdown code fences ```json ... ```
            if (raw.StartsWith("```"))
            {
                // remove first fence
                var idx = raw.IndexOf('\n');
                if (idx >= 0) raw = raw[(idx + 1)..];
                // remove trailing fence
                var end = raw.LastIndexOf("```", StringComparison.Ordinal);
                if (end >= 0) raw = raw[..end].Trim();
            }

            // If it already looks like JSON
            if (raw.StartsWith("{") && raw.EndsWith("}"))
                return raw;

            // Try to locate the first JSON object in the text
            var first = raw.IndexOf('{');
            var last = raw.LastIndexOf('}');
            if (first >= 0 && last > first)
            {
                return raw.Substring(first, last - first + 1).Trim();
            }

            return null;
        }
    }
}
