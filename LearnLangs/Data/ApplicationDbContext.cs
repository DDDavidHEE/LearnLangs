using LearnLangs.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<UserLesson> UserLessons => Set<UserLesson>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Lesson>()
                .HasIndex(l => new { l.CourseId, l.OrderIndex })
                .IsUnique();

            // Seed a little data so UI has something to show
            builder.Entity<Course>().HasData(new Course { Id = 1, Name = "Spanish – Beginner", Description = "Basics of Spanish" });

            builder.Entity<Lesson>().HasData(
                new Lesson { Id = 1, CourseId = 1, Title = "Greetings", OrderIndex = 1, XpReward = 10 },
                new Lesson { Id = 2, CourseId = 1, Title = "Numbers", OrderIndex = 2, XpReward = 10 }
            );

            builder.Entity<Question>().HasData(
                new Question { Id = 1, LessonId = 1, Prompt = "Hola = ?", IsMultipleChoice = true, OptionA = "Hello", OptionB = "Goodbye", OptionC = "Please", OptionD = "Thanks", CorrectAnswer = "A" },
                new Question { Id = 2, LessonId = 2, Prompt = "Dos = ?", IsMultipleChoice = true, OptionA = "One", OptionB = "Two", OptionC = "Three", OptionD = "Four", CorrectAnswer = "B" }
            );
        }
    }
}
