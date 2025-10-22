namespace LearnLangs.Options
{
    public class GeminiAiOptions
    {
        public string Model { get; set; } = "gemini-1.5-pro";
        public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
        public string? ApiVersion { get; set; } = "v1"; // 1.x/1.5 => v1; 2.x => v1beta
        public string? ApiKey { get; set; }
    }
}
