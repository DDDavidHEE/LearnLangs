using System;

namespace LearnLangs.Models.Dictation
{
    public class UserDictationProgress
    {
        public int Id { get; set; }

        // Khóa ngoại tới AspNetUsers
        public string UserId { get; set; } = "";

        // Khóa ngoại tới DictationSet
        public int SetId { get; set; }

        // Tiến độ
        public int LastIndex { get; set; } = 0;   // đã xong tới câu số mấy
        public int CorrectCount { get; set; } = 0;
        public int TotalCount { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // (tuỳ chọn) navigation
        // public ApplicationUser? User { get; set; }
        // public DictationSet? Set { get; set; }
    }
}
