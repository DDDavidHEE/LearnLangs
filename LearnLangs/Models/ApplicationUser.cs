using Microsoft.AspNetCore.Identity;
using System;

namespace LearnLangs.Models
{
    public class ApplicationUser : IdentityUser
    {
        // ===== Gamification / activity =====
        public int TotalXP { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public DateTime? LastActiveDate { get; set; }
        public bool Has7DayStreakReward { get; set; } = false;
        public bool Has30DayStreakReward { get; set; } = false;

        // ===== User Settings =====
        /// <summary>"light" | "dark"</summary>
        public string? PreferredUiTheme { get; set; } = "light";

        /// <summary>Mặc định ngôn ngữ nguồn cho Translate (vd: "auto")</summary>
        public string? DefaultTranslateFrom { get; set; } = "auto";

        /// <summary>Mặc định ngôn ngữ đích cho Translate (vd: "en")</summary>
        public string? DefaultTranslateTo { get; set; } = "en";

        /// <summary>Mặc định ngôn ngữ nhận dạng cho Pronunciation (vd: "en-US")</summary>
        public string? DefaultPronunciationLang { get; set; } = "en-US";

        /// <summary>Bật/tắt hiển thị JSON thô ở trang kết quả chấm phát âm</summary>
        public bool ShowPronunciationRawJson { get; set; } = false;
    }
}
