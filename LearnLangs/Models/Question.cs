using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Prompt { get; set; } = string.Empty;

        public bool IsMultipleChoice { get; set; } = true;

        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        // For MCQ: "A"/"B"/"C"/"D"; for short answer: the correct text
        [Required]
        public string CorrectAnswer { get; set; } = "A";
    }
}
