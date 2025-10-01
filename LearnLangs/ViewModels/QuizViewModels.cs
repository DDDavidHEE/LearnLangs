using System.Collections.Generic;

namespace LearnLangs.ViewModels
{
    // 1 câu hỏi để hiển thị trên UI
    public class QuizQuestionVM
    {
        public int QuestionId { get; set; }
        public string Prompt { get; set; } = "";
        public bool IsMultipleChoice { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        // đáp án người dùng nhập/chọn (A/B/C/D hoặc text)
        public string? UserAnswer { get; set; }
    }

    // ViewModel cho trang làm bài (TakeQuiz)
    public class TakeQuizVM
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = "";
        public List<QuizQuestionVM> Questions { get; set; } = new();
    }

    // ViewModel cho trang kết quả sau khi nộp bài
    public class QuizResultVM
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = "";
        public int Correct { get; set; }
        public int Total { get; set; }
        public int XpAwarded { get; set; }
    }
}
