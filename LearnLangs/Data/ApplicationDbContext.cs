using LearnLangs.Models;
using LearnLangs.Models.Dictation;
using LearnLangs.Models.Flashcards;
using LearnLangs.Models.Games;
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

        // ===== Flashcards =====
        public DbSet<FlashcardDeck> FlashcardDecks => Set<FlashcardDeck>();
        public DbSet<FlashcardCard> FlashcardCards => Set<FlashcardCard>();
        // Nếu có Category: public DbSet<FlashcardCategory> FlashcardCategories => Set<FlashcardCategory>();

        // ===== Games & Exams =====
        public DbSet<GameLevel> GameLevels => Set<GameLevel>();
        public DbSet<GameQuestion> GameQuestions => Set<GameQuestion>();
        public DbSet<GameResult> GameResults => Set<GameResult>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------------- Core Learning ----------------
            builder.Entity<Lesson>()
                .HasIndex(l => new { l.CourseId, l.OrderIndex })
                .IsUnique();

            builder.Entity<UserLesson>()
                .HasIndex(ul => new { ul.UserId, ul.LessonId })
                .IsUnique();

            // ---------------- Dictation ----------------
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

            builder.Entity<DictationSet>()
                .HasIndex(s => new { s.TopicId, s.OrderIndex });

            builder.Entity<DictationItem>()
                .HasIndex(i => new { i.SetId, i.OrderIndex });

            builder.Entity<UserDictationProgress>()
                .HasIndex(p => new { p.UserId, p.SetId })
                .IsUnique();

            // ---------------- Flashcards ----------------
            builder.Entity<FlashcardCard>()
                .HasOne(c => c.Deck)
                .WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FlashcardDeck>()
                .HasIndex(d => new { d.Mode, d.OrderIndex });

            builder.Entity<FlashcardCard>()
                .HasIndex(c => new { c.DeckId, c.OrderIndex });

            // ---------------- Games ----------------
            builder.Entity<GameLevel>()
                .HasMany(l => l.Questions)
                .WithOne(q => q.GameLevel)
                .HasForeignKey(q => q.GameLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GameResult>()
                .HasIndex(r => new { r.UserId, r.GameLevelId });

            // ---------------- Exams ----------------
            builder.Entity<Exam>()
                .HasOne(e => e.GameLevel)
                .WithMany()
                .HasForeignKey(e => e.GameLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamAttempt>()
                .HasOne(a => a.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(a => a.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamAttempt>()
                .HasIndex(a => new { a.UserId, a.ExamId });

            // ---------------- Seed dữ liệu demo ----------------
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

            // ====== Chinese course (demo) ======
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
                new Question { Id = 1001, LessonId = 101, Prompt = "“你好” nghĩa là gì?", IsMultipleChoice = true, OptionA = "Tạm biệt", OptionB = "Xin chào", OptionC = "Cảm ơn", OptionD = "Xin lỗi", CorrectAnswer = "B" },
                new Question { Id = 1002, LessonId = 101, Prompt = "“早上好” nghĩa là…", IsMultipleChoice = true, OptionA = "Chào buổi sáng", OptionB = "Chúc ngủ ngon", OptionC = "Chúc mừng", OptionD = "Hẹn gặp lại", CorrectAnswer = "A" },
                new Question { Id = 1003, LessonId = 101, Prompt = "“你好吗？” nghĩa là…", IsMultipleChoice = true, OptionA = "Bạn tên gì?", OptionB = "Bạn khỏe không?", OptionC = "Bạn ở đâu?", OptionD = "Bạn bao nhiêu tuổi?", CorrectAnswer = "B" }
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
