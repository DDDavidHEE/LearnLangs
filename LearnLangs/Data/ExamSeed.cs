using LearnLangs.Models.Games;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Data
{
    public static class ExamSeed
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Exams.AnyAsync()) return;

            // giả sử level1 id = 1
            db.Exams.AddRange(
                new Exam { Title = "Làm bài thi 1", Order = 1, GameLevelId = 1 },
                new Exam { Title = "Làm bài thi 2", Order = 2, GameLevelId = 1 },
                new Exam { Title = "Làm bài thi 3", Order = 3, GameLevelId = 1 },
                new Exam { Title = "Làm bài thi 4", Order = 4, GameLevelId = 1 }
            );
            await db.SaveChangesAsync();
        }
    }
}
