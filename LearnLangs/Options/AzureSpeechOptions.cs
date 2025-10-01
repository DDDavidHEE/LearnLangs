
namespace LearnLangs.Options
{  
    public class AzureSpeechOptions
    {
        public string SubscriptionKey { get; set; } = string.Empty;
        public string Region { get; set; } = "southeastasia";   
        public string RecognitionLanguage { get; set; } = "en-US";  
        public string? Endpoint { get; set; }
    }
}
