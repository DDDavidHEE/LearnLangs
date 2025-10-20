using System.Threading.Tasks;

namespace LearnLangs.Services.Pronunciation
{
    public interface IAzureSpeechService
    {
        Task<string> RecognizeAsync(byte[] audioBytes);
    }

}
