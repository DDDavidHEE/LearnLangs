using System.Threading.Tasks;

namespace LearnLangs.Services.Pronunciation
{
    public interface IPronunciationAnalyzerService
    {
        Task<PronunciationAnalysisResult> AnalyzePronunciationAsync(byte[] audioBytes);
    }
}
