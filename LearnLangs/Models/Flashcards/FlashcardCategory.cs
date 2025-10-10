using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Flashcards
{
    public class FlashcardCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; } = "";      // "common" | "ielts" | "hsk" ...

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";     // Tên hiển thị: "English Common", "IELTS", "HSK"

        public int OrderIndex { get; set; } = 0;    // Dùng để sắp xếp

        // Nav
        public List<FlashcardSet> Sets { get; set; } = new();
    }
}
