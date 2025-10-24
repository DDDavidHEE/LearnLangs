namespace LearnLangs.Models.Games
{
    public class GameResult
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public int GameLevelId { get; set; }
        public int TotalQuestions { get; set; }
        public int Correct { get; set; }
        public int Score { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
