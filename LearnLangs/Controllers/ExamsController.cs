using System.Security.Claims;
using LearnLangs.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Controllers
{
    [Authorize]
    [Route("exams")]
    public class ExamsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ExamsController(ApplicationDbContext db) => _db = db;
        private string Uid() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public sealed record ExamRowVm(
            int Id, string Title, int Order,
            int Attempts, int LastScore, int LastDuration,
            int BestScore, int BestDuration);

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var uid = Uid();
            var rows = await _db.Exams
                .Where(e => e.IsActive)
                .OrderBy(e => e.Order)
                .Select(e => new ExamRowVm(
                    e.Id, e.Title, e.Order,
                    e.Attempts.Where(a => a.UserId == uid).Count(),
                    e.Attempts.Where(a => a.UserId == uid).OrderByDescending(a => a.CreatedUtc).Select(a => a.Score).FirstOrDefault(),
                    e.Attempts.Where(a => a.UserId == uid).OrderByDescending(a => a.CreatedUtc).Select(a => a.DurationSeconds).FirstOrDefault(),
                    e.Attempts.Where(a => a.UserId == uid).OrderByDescending(a => a.Score).Select(a => a.Score).FirstOrDefault(),
                    e.Attempts.Where(a => a.UserId == uid).OrderByDescending(a => a.Score).Select(a => a.DurationSeconds).FirstOrDefault()
                ))
                .ToListAsync();

            return View(rows);
        }

        [HttpGet("start/{id:int}")]
        public async Task<IActionResult> Start(int id)
        {
            var exam = await _db.Exams.Include(e => e.GameLevel).FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (exam == null) return NotFound();
            return Redirect($"/games/play/{exam.GameLevelId}?examId={exam.Id}");
        }
    }
}
