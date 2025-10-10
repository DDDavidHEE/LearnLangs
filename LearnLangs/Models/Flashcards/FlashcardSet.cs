using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Flashcards
{
    public class FlashcardSet
    {
        public int Id { get; set; }

        // FK tới Category
        public int CategoryId { get; set; }
        public FlashcardCategory? Category { get; set; }

        [Required, MaxLength(100)]
        public string SetCode { get; set; } = "";   // Ví dụ: EN-COMMON-SCHOOL, IELTS-65-FJ, HSK3-P2

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";     // "School", "IELTS 6.5 – F-J", "HSK3 – Phần 2"

        public int Part { get; set; } = 1;          // Phần (nếu tách)

        public int OrderIndex { get; set; } = 0;    // Thứ tự hiển thị trong Category

        [MaxLength(300)]
        public string? CoverUrl { get; set; }       // Ảnh cover bộ từ

        [MaxLength(20)]
        public string? Lang { get; set; }           // "en-GB", "zh-CN", ...

        public bool IsAdminOwned { get; set; } = true; // Bộ mặc định do admin tạo?

        // Nav
        public List<FlashcardCard> Cards { get; set; } = new();
    }
}
