using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using LearnLangs.Options;
using Microsoft.Extensions.Options;

namespace LearnLangs.Services.Translate
{
    /// <summary>Wrapper đơn giản cho Azure Translator v3</summary>
    public class AzureTranslatorService : ITranslateService
    {
        private readonly HttpClient _http;
        private readonly AzureTranslatorOptions _opt;

        public AzureTranslatorService(HttpClient http, IOptions<AzureTranslatorOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task<TranslateResponse> TranslateAsync(string text, string? from, string to)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new TranslateResponse("", "");

            var url = $"{_opt.Endpoint.TrimEnd('/')}/translate?api-version=3.0&to={Uri.EscapeDataString(to)}";
            if (!string.IsNullOrWhiteSpace(from) && from != "auto")
                url += $"&from={Uri.EscapeDataString(from)}";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("Ocp-Apim-Subscription-Key", _opt.SubscriptionKey);
            req.Headers.Add("Ocp-Apim-Subscription-Region", _opt.Region);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new[] { new { Text = text } }),
                Encoding.UTF8,
                "application/json"
            );

            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement[0];
            var detected = root.GetProperty("detectedLanguage").GetProperty("language").GetString() ?? "";
            var translated = root.GetProperty("translations")[0].GetProperty("text").GetString() ?? "";
            return new TranslateResponse(detected, translated);
        }
    }
}
