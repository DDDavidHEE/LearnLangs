using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace LearnLangs.Services.Pronunciation
{
    public class PronunciationAnalyzerService : IPronunciationAnalyzerService
    {
        private readonly IConfiguration _config;
        private readonly IAzureSpeechService _speechService; // Bạn cần tự cài đặt service này

        public PronunciationAnalyzerService(IConfiguration config, IAzureSpeechService speechService)
        {
            _config = config;
            _speechService = speechService;
        }

        public async Task<PronunciationAnalysisResult> AnalyzePronunciationAsync(byte[] audioBytes)
        {
            // 1. Chuyển audio sang text
            var recognizedText = await _speechService.RecognizeAsync(audioBytes);

            // 2. Gửi text lên Gemini để phân tích
            var geminiResult = await AnalyzeTextWithGeminiAsync(recognizedText);

            return new PronunciationAnalysisResult
            {
                RecognizedText = recognizedText,
                GrammarCorrections = geminiResult.GrammarCorrections,
                StyleImprovements = geminiResult.StyleImprovements,
                Suggestions = geminiResult.Suggestions
            };
        }

        private async Task<GeminiAnalysisResult> AnalyzeTextWithGeminiAsync(string text)
        {
            var apiKey = _config["GeminiAI:ApiKey"];
            var endpoint = _config["GeminiAI:Endpoint"];
            var model = _config["GeminiAI:Model"];

            var prompt = $"Check pronunciation, grammar, and suggest better phrasing for: {text}";

            var requestBody = new
            {
                model,
                prompt
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var response = await client.PostAsync(endpoint, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiAnalysisResult>();
            return result;
        }
    }

    public class GeminiAnalysisResult
    {
        public string GrammarCorrections { get; set; }
        public string StyleImprovements { get; set; }
        public string Suggestions { get; set; }
    }
}
