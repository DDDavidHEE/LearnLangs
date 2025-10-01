namespace LearnLangs.Models
{
    public class SettingsViewModel
    {
        // UI
        public string PreferredUiTheme { get; set; } = "light";

        // Translate
        public string From { get; set; } = "auto";
        public string To { get; set; } = "en";

        // Pronunciation
        public string PronunciationLang { get; set; } = "en-US";
        public bool ShowPronunciationRawJson { get; set; } = false;
    }
}
