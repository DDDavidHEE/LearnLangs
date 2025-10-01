namespace LearnLangs.Services.Translate
{
    public record TranslateResponse(string DetectedLanguage, string TranslatedText);

    public interface ITranslateService
    {
        Task<TranslateResponse> TranslateAsync(string text, string? from, string to);
    }
}
