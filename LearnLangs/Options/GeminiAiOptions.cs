namespace LearnLangs.Options
{
    public class GeminiAiOptions
    {
        public string Model { get; set; } = "gemini-1.5-pro";   // safer default
        public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
        public string ApiVersion { get; set; } = "v1beta";       // make version explicit
        public string ApiKey { get; set; } = "";                 // from user-secrets
    }
}
