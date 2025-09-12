using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
