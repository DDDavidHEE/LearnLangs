// LearnLangs/Services/Pronunciation/IPronunciationAssessmentService.cs
using Microsoft.AspNetCore.Http;

namespace LearnLangs.Services.Pronunciation;

public interface IPronunciationAssessmentService
{
    /// <summary>
    /// Đánh giá phát âm cho file WAV người dùng tải lên so với câu tham chiếu.
    /// </summary>
    Task<PronunciationResultDto> AssessAsync(
        string referenceText,
        IFormFile wavFile,
        CancellationToken ct = default);
}
