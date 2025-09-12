using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        public int OrderIndex { get; set; }
        public int XpReward { get; set; } = 10;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<UserLesson> UserLessons { get; set; } = new List<UserLesson>();
    }
}
