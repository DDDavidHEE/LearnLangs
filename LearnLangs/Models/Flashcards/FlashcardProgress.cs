using System;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Flashcards
{
    public class FlashcardProgress
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public int SetId { get; set; }

        public int KnownCount { get; set; } = 0;     // số từ user bấm "Đã biết"
        public int LearnedCount { get; set; } = 0;   // tổng từ đã học (hoặc hoàn thành set)

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
