using System;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class UserLesson
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;

        [Required]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = default!;

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedOn { get; set; }
        public int Score { get; set; } = 0;
    }
}
