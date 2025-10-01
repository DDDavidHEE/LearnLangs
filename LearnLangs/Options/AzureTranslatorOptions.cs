namespace LearnLangs.Options
{
    /// <summary>
    /// Cấu hình Azure Translator (bind từ section "AzureTranslator").
    /// SubscriptionKey lấy qua User Secrets: AzureTranslator:SubscriptionKey
    /// </summary>
    public class AzureTranslatorOptions
    {
        public string Endpoint { get; set; } = "https://api.cognitive.microsofttranslator.com";
        public string Region { get; set; } = "southeastasia";
        public string SubscriptionKey { get; set; } = string.Empty;

        // Ngôn ngữ đích mặc định (nếu controller không truyền)
        public string DefaultTo { get; set; } = "en";
    }
}
