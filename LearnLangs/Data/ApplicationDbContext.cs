using LearnLangs.Models;
using LearnLangs.Models.Dictation; // Dictation models
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ===== Core learning =====
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<UserLesson> UserLessons => Set<UserLesson>();

        // ===== Dictation =====
        public DbSet<DictationTopic> DictationTopics => Set<DictationTopic>();
        public DbSet<DictationSet> DictationSets => Set<DictationSet>();
        public DbSet<DictationItem> DictationItems => Set<DictationItem>();
        public DbSet<UserDictationProgress> UserDictationProgresses => Set<UserDictationProgress>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------------- Indexes & constraints ----------------
            // Lesson: một course không được trùng OrderIndex
            builder.Entity<Lesson>()
                .HasIndex(l => new { l.CourseId, l.OrderIndex })
                .IsUnique();

            // UserLesson: 1 user – 1 lesson duy nhất
            builder.Entity<UserLesson>()
                .HasIndex(ul => new { ul.UserId, ul.LessonId })
                .IsUnique();

            // Dictation quan hệ: Topic 1-n Set, Set 1-n Item
            builder.Entity<DictationSet>()
                .HasOne(s => s.Topic)
                .WithMany(t => t.Sets)
                .HasForeignKey(s => s.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DictationItem>()
                .HasOne(i => i.Set)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index sắp xếp nhanh
            builder.Entity<DictationSet>()
                .HasIndex(s => new { s.TopicId, s.OrderIndex });

            builder.Entity<DictationItem>()
                .HasIndex(i => new { i.SetId, i.OrderIndex });

            // Mỗi user chỉ có 1 progress cho mỗi Set
            builder.Entity<UserDictationProgress>()
                .HasIndex(p => new { p.UserId, p.SetId })
                .IsUnique();

            // ---------------- Seed demo courses (giữ nguyên) ----------------
            builder.Entity<Course>().HasData(
                new Course { Id = 1, Name = "Spanish – Beginner", Description = "Basics of Spanish" }
            );

            builder.Entity<Lesson>().HasData(
                new Lesson { Id = 1, CourseId = 1, Title = "Greetings", OrderIndex = 1, XpReward = 10 },
                new Lesson { Id = 2, CourseId = 1, Title = "Numbers", OrderIndex = 2, XpReward = 10 }
            );

            builder.Entity<Question>().HasData(
                new Question { Id = 1, LessonId = 1, Prompt = "Hola = ?", IsMultipleChoice = true, OptionA = "Hello", OptionB = "Goodbye", OptionC = "Please", OptionD = "Thanks", CorrectAnswer = "A" },
                new Question { Id = 2, LessonId = 2, Prompt = "Dos = ?", IsMultipleChoice = true, OptionA = "One", OptionB = "Two", OptionC = "Three", OptionD = "Four", CorrectAnswer = "B" }
            );

            // ====== Seed: Chinese course (demo) ======
            const int chineseCourseId = 100;
            builder.Entity<Course>().HasData(
                new Course { Id = chineseCourseId, Name = "Chinese course", Description = "Beginner Mandarin: greetings, numbers, self-intro with pinyin." }
            );

            builder.Entity<Lesson>().HasData(
                new Lesson { Id = 101, CourseId = chineseCourseId, Title = "Lesson 1 – Greetings", OrderIndex = 1, XpReward = 30 },
                new Lesson { Id = 102, CourseId = chineseCourseId, Title = "Lesson 2 – Numbers 1–10", OrderIndex = 2, XpReward = 30 },
                new Lesson { Id = 103, CourseId = chineseCourseId, Title = "Lesson 3 – Self-Introduction", OrderIndex = 3, XpReward = 40 }
            );

            builder.Entity<Question>().HasData(
                // L1 – Greetings
                new Question { Id = 1001, LessonId = 101, Prompt = "“你好” nghĩa là gì?", IsMultipleChoice = true, OptionA = "Tạm biệt", OptionB = "Xin chào", OptionC = "Cảm ơn", OptionD = "Xin lỗi", CorrectAnswer = "B" },
                new Question { Id = 1002, LessonId = 101, Prompt = "“早上好” nghĩa là…", IsMultipleChoice = true, OptionA = "Chào buổi sáng", OptionB = "Chúc ngủ ngon", OptionC = "Chúc mừng", OptionD = "Hẹn gặp lại", CorrectAnswer = "A" },
                new Question { Id = 1003, LessonId = 101, Prompt = "“你好吗？” nghĩa là…", IsMultipleChoice = true, OptionA = "Bạn tên gì?", OptionB = "Bạn khỏe không?", OptionC = "Bạn ở đâu?", OptionD = "Bạn bao nhiêu tuổi?", CorrectAnswer = "B" },
                new Question { Id = 1004, LessonId = 101, Prompt = "Trả lời lịch sự cho “你好吗？”", IsMultipleChoice = true, OptionA = "我很好，谢谢。", OptionB = "对不起。", OptionC = "再见。", OptionD = "没关系。", CorrectAnswer = "A" },
                new Question { Id = 1005, LessonId = 101, Prompt = "“再见” nghĩa là…", IsMultipleChoice = true, OptionA = "Cảm ơn", OptionB = "Tạm biệt", OptionC = "Xin chào", OptionD = "Không sao", CorrectAnswer = "B" },

                // L2 – Numbers
                new Question { Id = 1011, LessonId = 102, Prompt = "Số “四” là số nào?", IsMultipleChoice = true, OptionA = "3", OptionB = "4", OptionC = "5", OptionD = "6", CorrectAnswer = "B" },
                new Question { Id = 1012, LessonId = 102, Prompt = "Pinyin đúng của “八” là…", IsMultipleChoice = true, OptionA = "bā", OptionB = "bá", OptionC = "bǎ", OptionD = "bà", CorrectAnswer = "A" },
                new Question { Id = 1013, LessonId = 102, Prompt = "“九” là…", IsMultipleChoice = true, OptionA = "6", OptionB = "7", OptionC = "8", OptionD = "9", CorrectAnswer = "D" },
                new Question { Id = 1014, LessonId = 102, Prompt = "“liù” viết Hán tự là…", IsMultipleChoice = true, OptionA = "六", OptionB = "九", OptionC = "二", OptionD = "十", CorrectAnswer = "A" },
                new Question { Id = 1015, LessonId = 102, Prompt = "Số “10” trong tiếng Trung là…", IsMultipleChoice = true, OptionA = "百", OptionB = "千", OptionC = "十", OptionD = "万", CorrectAnswer = "C" },

                // L3 – Self-Introduction
                new Question { Id = 1021, LessonId = 103, Prompt = "“我叫…” dùng khi nào?", IsMultipleChoice = true, OptionA = "Nói tuổi", OptionB = "Nói quê quán", OptionC = "Nói tên", OptionD = "Nói nghề nghiệp", CorrectAnswer = "C" },
                new Question { Id = 1022, LessonId = 103, Prompt = "Câu hỏi để hỏi tên người khác:", IsMultipleChoice = true, OptionA = "你叫什么名字？", OptionB = "你多大？", OptionC = "你来自哪里？", OptionD = "你做什么工作？", CorrectAnswer = "A" },
                new Question { Id = 1023, LessonId = 103, Prompt = "“我来自越南。” nghĩa là…", IsMultipleChoice = true, OptionA = "Tôi đến từ Việt Nam", OptionB = "Tôi yêu Việt Nam", OptionC = "Tôi ở Việt Nam", OptionD = "Tôi nói tiếng Việt", CorrectAnswer = "A" },
                new Question { Id = 1024, LessonId = 103, Prompt = "“我是学生。” tương đương…", IsMultipleChoice = true, OptionA = "I am a teacher", OptionB = "I am a student", OptionC = "I am from China", OptionD = "My name is…", CorrectAnswer = "B" },
                new Question { Id = 1025, LessonId = 103, Prompt = "“很高兴认识你。” nghĩa là…", IsMultipleChoice = true, OptionA = "Cảm ơn", OptionB = "Xin lỗi", OptionC = "Rất vui được gặp bạn", OptionD = "Hẹn gặp lại", CorrectAnswer = "C" }
            );

            // ====== Seed Dictation DEMO (tuỳ chọn) ======
            var topicId = 2000;
            var setId = 2100;

            builder.Entity<DictationTopic>().HasData(new DictationTopic
            {
                Id = topicId,
                Title = "IELTS Listening (Demo)",
                Description = "Mini demo",
                CoverUrl = "/img/ielts.png"
            });

            builder.Entity<DictationSet>().HasData(new DictationSet
            {
                Id = setId,
                TopicId = topicId,
                Title = "Cam 20 – Test 1 – Part 1 (Demo)",
                Level = "B2",
                OrderIndex = 1
            });

            builder.Entity<DictationItem>().HasData(
                new DictationItem
                {
                    Id = 2101,
                    SetId = setId,
                    OrderIndex = 1,
                    Transcript = "I've been meaning to ask you for some advice about restaurants.",
                    AudioUrl = "/audio/demo1.mp3"
                },
                new DictationItem
                {
                    Id = 2102,
                    SetId = setId,
                    OrderIndex = 2,
                    Transcript = "I need to book somewhere to celebrate my sister's thirtieth birthday.",
                    AudioUrl = "/audio/demo2.mp3"
                }
            );
        }
    }
}
