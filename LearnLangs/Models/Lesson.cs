using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        // Không cho trùng trong cùng Course (sẽ check ở Controller)
        [Range(1, 1000, ErrorMessage = "Order must be between 1 and 1000.")]
        public int OrderIndex { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "XP must be between 1 and 1000.")]
        public int XpReward { get; set; } = 10;

        public Course? Course { get; set; }
        public ICollection<Question>? Questions { get; set; }
    }
}
