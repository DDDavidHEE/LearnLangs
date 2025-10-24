using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Games
{
    public class GameQuestion
    {
        public int Id { get; set; }

        public int GameLevelId { get; set; }
        public GameLevel GameLevel { get; set; } = default!;

        public GameType Type { get; set; }

        // Dùng tùy kiểu game
        [MaxLength(256)] public string? Prompt { get; set; }
        [MaxLength(256)] public string? ImageUrl { get; set; }
        [MaxLength(256)] public string? CorrectText { get; set; }
        [MaxLength(256)] public string? SentenceShuffled { get; set; }
        [MaxLength(256)] public string? SentenceAnswer { get; set; }

        // Matching (JSON)
        public string? OptionsJson { get; set; }
        public string? ImagesJson { get; set; }
        public string? AnswerMapJson { get; set; }
    }
}
