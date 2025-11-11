using System.Security.Claims;
using System.Text.Json;
using LearnLangs.Data;
using LearnLangs.Services.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Controllers
{
    [Authorize]
    [Route("games")]
    public class GamesController : Controller
    {
        private readonly GameService _svc;
        private readonly ApplicationDbContext _db;

        public GamesController(GameService svc, ApplicationDbContext db)
        {
            _svc = svc;
            _db = db;
        }

        private string Uid() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET /games  -> hiển thị danh sách bài thi
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var exams = await _db.Exams
                                 .OrderBy(e => e.Order)
                                 .ToListAsync();
            return View(exams); // Views/Games/Index.cshtml (model: IEnumerable<Exam>)
        }

        // GET /games/play/{id}
        // id có thể là LevelId (mở trực tiếp) hoặc ExamId (redirect sang level tương ứng)
        [HttpGet("play/{id:int}")]
        public async Task<IActionResult> Play(int id)
        {
            // 1) Thử coi id là LevelId
            var level = await _svc.GetLevelAsync(id);
            if (level != null)
                return View("PlaySpace", level); // Views/Games/PlaySpace.cshtml (model: GameLevel)

            // 2) Không phải LevelId -> thử coi là ExamId, rồi redirect sang Level
            var exam = await _db.Exams
                                .AsNoTracking()
                                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (exam != null)
            {
                // redirect để giữ logic Play theo level, đồng thời mang theo examId để lưu Attempt
                return Redirect($"/games/play/{exam.GameLevelId}?examId={exam.Id}");
            }

            return NotFound(); // không tìm thấy Level/Exam
        }

        // Ajax chấm 1 câu
        [HttpPost("grade/{qid:int}")]
        public async Task<IActionResult> Grade(int qid, [FromQuery] int levelId, [FromBody] JsonElement body)
        {
            var level = await _svc.GetLevelAsync(levelId);
            if (level == null) return NotFound();

            var q = level.Questions.FirstOrDefault(x => x.Id == qid);
            if (q == null) return NotFound();

            var payload = body.TryGetProperty("payload", out var p) ? (p.GetString() ?? "") : "";
            var (correct, score) = _svc.Grade(q, payload);
            return Json(new { correct, score });
        }

        public record GameFinishDto(int LevelId, int Total, int Correct, int Score, int Seconds);

        [HttpPost("finish")]
        public async Task<IActionResult> Finish([FromBody] GameFinishDto dto)
        {
            await _svc.SaveResultAsync(
                Uid(),
                dto.LevelId,
                dto.Total,
                dto.Correct,
                dto.Score,
                TimeSpan.FromSeconds(dto.Seconds)
            );

            // Nếu có examId -> lưu thêm ExamAttempt
            if (Request.Query.TryGetValue("examId", out var s) && int.TryParse(s, out var examId))
            {
                _db.ExamAttempts.Add(new Models.Games.ExamAttempt
                {
                    ExamId = examId,
                    UserId = Uid(),
                    Score = dto.Score,
                    DurationSeconds = dto.Seconds,
                    CreatedUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        // ===========================
        //  BLOCKS MINI-GAME (độc lập)
        // ===========================

        // GET /games/blocks  -> hiển thị trò chơi Blocks
        [HttpGet("blocks")]
        public IActionResult Blocks()
        {
            // View tương ứng: Views/Games/Blocks.cshtml
            return View();
        }
    }
}
