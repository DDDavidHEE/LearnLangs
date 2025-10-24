using LearnLangs.Data;
using LearnLangs.Models.Games;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Services.Games
{
    public class GameService
    {
        private readonly ApplicationDbContext _db;
        public GameService(ApplicationDbContext db) { _db = db; }

        public async Task<GameLevel?> GetLevelAsync(int levelId) =>
            await _db.GameLevels.Include(l => l.Questions)
                                .FirstOrDefaultAsync(l => l.Id == levelId && l.IsActive);

        public (bool correct, int score) Grade(GameQuestion q, string payload)
        {
            switch (q.Type)
            {
                case GameType.FillInBlank:
                    return (string.Equals(payload.Trim(), q.CorrectText, StringComparison.OrdinalIgnoreCase), 10);

                case GameType.SentenceOrdering:
                    return ((payload ?? "").Trim() == (q.SentenceAnswer ?? "").Trim(), 10);

                case GameType.MatchImageMeaning:
                    // tạm thời chưa dùng ở view này
                    return (false, 0);

                default: return (false, 0);
            }
        }

        public async Task SaveResultAsync(string userId, int levelId, int total, int correct, int score, TimeSpan duration)
        {
            _db.GameResults.Add(new GameResult
            {
                UserId = userId,
                GameLevelId = levelId,
                TotalQuestions = total,
                Correct = correct,
                Score = score,
                Duration = duration
            });
            await _db.SaveChangesAsync();
        }
    }
}
