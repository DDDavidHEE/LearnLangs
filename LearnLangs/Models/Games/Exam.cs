using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Games
{
    public class Exam
    {
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string Title { get; set; } = default!;

        public int Order { get; set; } = 1;

        // map 1 bài thi ↔ 1 level
        public int GameLevelId { get; set; }
        public GameLevel GameLevel { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public ICollection<ExamAttempt> Attempts { get; set; } = new List<ExamAttempt>();
    }

    public class ExamAttempt
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public int Score { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
