using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LearnLangs.Models;

namespace LearnLangs.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Áp dụng migration nếu cần
            await context.Database.MigrateAsync();

            // === Course ===
            var course = await context.Courses.FirstOrDefaultAsync(c => c.Name == "Spanish Basics");
            if (course == null)
            {
                course = new Course
                {
                    Name = "Spanish Basics",
                    Description = "Greetings and essentials"
                };
                context.Courses.Add(course);
                await context.SaveChangesAsync();
            }

            // === Lesson ===
            var lesson = await context.Lessons
                .FirstOrDefaultAsync(l => l.CourseId == course.Id && l.Title == "Greetings");
            if (lesson == null)
            {
                lesson = new Lesson
                {
                    CourseId = course.Id,
                    Title = "Greetings",
                    OrderIndex = 1,
                    XpReward = 10
                };
                context.Lessons.Add(lesson);
                await context.SaveChangesAsync();
            }

            // === Questions (idempotent theo Prompt) ===
            async Task EnsureMcq(string prompt, string a, string b, string c, string d, string correct)
            {
                var exists = await context.Questions
                    .AnyAsync(q => q.LessonId == lesson.Id && q.Prompt == prompt);
                if (exists) return;

                context.Questions.Add(new Question
                {
                    LessonId = lesson.Id,
                    Prompt = prompt,
                    IsMultipleChoice = true,
                    OptionA = a,
                    OptionB = b,
                    OptionC = c,
                    OptionD = d,
                    CorrectAnswer = correct
                });
            }

            await EnsureMcq("Hola = ?", "Hello", "Goodbye", "Please", "Thanks", "A");
            await EnsureMcq("Adiós = ?", "Thanks", "Goodbye", "Please", "Hello", "B");
            await EnsureMcq("Gracias = ?", "Please", "Goodbye", "Thanks", "See you", "C");

            await context.SaveChangesAsync();
        }
    }
}
