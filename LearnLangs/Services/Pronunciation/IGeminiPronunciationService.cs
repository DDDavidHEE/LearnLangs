namespace LearnLangs.Services.Pronunciation
{
    public interface IGeminiPronunciationService
    {
        Task<PronunciationResultDto> AssessWithGeminiAsync(
            byte[] audioBytes,
            string referenceText,
            string mimeType = "audio/webm",
            CancellationToken ct = default);
    }
}
