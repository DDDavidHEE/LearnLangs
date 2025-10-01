// LearnLangs/Services/Pronunciation/PronunciationDtos.cs
namespace LearnLangs.Services.Pronunciation;

public record WordScoreDto(string Word, string ErrorType, double AccuracyScore);

public record PronunciationResultDto(
    double PronunciationScore,
    double AccuracyScore,
    double FluencyScore,
    double CompletenessScore,
    double? ProsodyScore,
    IReadOnlyList<WordScoreDto> Words,
    string RawJson);