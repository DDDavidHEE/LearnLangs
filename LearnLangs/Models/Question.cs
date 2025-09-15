using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class Question
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key to Lesson
        [Required]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = default!;

        // Question prompt text
        [Required, MaxLength(300)]
        public string Prompt { get; set; } = string.Empty;

        // Indicates if the question is multiple choice
        public bool IsMultipleChoice { get; set; } = true;

        // Multiple choice options (A, B, C, D)
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        // Correct answer (for multiple choice: "A"/"B"/"C"/"D", for short answer: the correct text)
        [Required]
        public string CorrectAnswer { get; set; } = "A";

        // Short answer question answer (if not multiple choice)
        public string? ShortAnswer { get; set; }

        // For fill-in-the-blank type of questions
        public string? FillInTheBlankAnswer { get; set; }
    }
}
