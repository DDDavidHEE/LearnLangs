using LearnLangs.Models;                     
using LearnLangs.Models.Dictation;
using LearnLangs.Services.Translate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;         
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Controllers
{
    [Authorize]
    public class DictationController : Controller
    {
        private readonly Data.ApplicationDbContext _db;
        private readonly ITranslateService _translate;
        private readonly UserManager<ApplicationUser> _userManager;   

        public DictationController(
            Data.ApplicationDbContext db,
            ITranslateService translate,
            UserManager<ApplicationUser> userManager)                  
        {
            _db = db;
            _translate = translate;
            _userManager = userManager;                                
        }

        // List topics
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var topics = await _db.DictationTopics
                .Include(t => t.Sets)
                .OrderBy(t => t.Title)
                .ToListAsync();
            return View(topics);
        }

        // List sets in a topic
        [HttpGet]
        public async Task<IActionResult> Topic(int id)
        {
            var topic = await _db.DictationTopics
                .Include(t => t.Sets)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null) return NotFound();
            return View(topic);
        }

        // Exercise page
        [HttpGet]
        public async Task<IActionResult> Set(int id, int index = 1)
        {
            var set = await _db.DictationSets
                .Include(s => s.Topic)
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (set == null) return NotFound();

            // Sắp xếp Items theo OrderIndex
            set.Items = set.Items.OrderBy(i => i.OrderIndex).ToList();

            ViewBag.Index = Math.Max(1, index);
            return View(set);
        }

        // API: lấy item theo index (1-based)
        [HttpGet]
        public async Task<IActionResult> Item(int setId, int index)
        {
            var item = await _db.DictationItems
                .Where(i => i.SetId == setId)
                .OrderBy(i => i.OrderIndex)
                .Skip(index - 1).Take(1)
                .Select(i => new
                {
                    i.Id,
                    Order = i.OrderIndex,
                    i.AudioUrl
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();
            return Json(item);
        }

        // =========================
        // API: chấm + trả về gợi ý
        // =========================
        [HttpPost]
        public async Task<IActionResult> Check([FromForm] int itemId, [FromForm] string answer)
        {
            var item = await _db.DictationItems.FindAsync(itemId);
            if (item == null) return NotFound();

            var expected = item.Transcript ?? "";
            var user = answer ?? "";

            bool correct = Normalize(user) == Normalize(expected);

            if (correct) return Json(new { correct = true });

            var hint = ComputeHint(expected, user);
            return Json(new { correct = false, hint });
        }

        // API: dịch nghĩa khi đúng
        [HttpGet]
        public async Task<IActionResult> Translate(string text)
        {
            var res = await _translate.TranslateAsync(text ?? "", "auto", "vi");
            return Json(new { detected = res.DetectedLanguage, vi = res.TranslatedText });
        }

        // =========================
        // API: hoàn thành set -> cộng XP
        // =========================
        [HttpPost]
        public async Task<IActionResult> Complete(int setId)
        {
            // đơn giản: cộng thẳng 20 XP khi người dùng hoàn thành set
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();

            me.TotalXP += 20;
            await _userManager.UpdateAsync(me);

            // Có thể mở rộng: lưu cờ đã nhận thưởng cho setId để tránh cộng nhiều lần
            return Json(new { ok = true, totalXp = me.TotalXP });
        }

        // ============ Helpers ============

        private static string Normalize(string s)
        {
            s ??= "";
            s = s.ToLowerInvariant();
            s = new string(s.Where(ch => !char.IsPunctuation(ch)).ToArray());
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        private static List<(string raw, string norm)> Tokenize(string s)
        {
            var list = new List<(string raw, string norm)>();
            foreach (System.Text.RegularExpressions.Match m
                     in System.Text.RegularExpressions.Regex.Matches(s, @"\b[\w']+\b"))
            {
                var raw = m.Value;
                var norm = Normalize(raw);
                list.Add((raw, norm));
            }
            return list;
        }

        /// <summary>
        /// Trả về gợi ý cho từ sai/thiếu đầu tiên:
        /// index (0-based), preview (1–2 ký tự đầu), length (độ dài từ), reason (mô tả ngắn).
        /// </summary>
        private static object ComputeHint(string expected, string user)
        {
            var exTokens = Tokenize(expected);
            var usTokens = Tokenize(user);

            int i = 0;
            var min = Math.Min(exTokens.Count, usTokens.Count);

            while (i < min && exTokens[i].norm == usTokens[i].norm) i++;

            if (i < exTokens.Count && i < usTokens.Count)
            {
                var exp = exTokens[i].raw;
                return new
                {
                    index = i,
                    preview = exp[..Math.Min(2, exp.Length)],
                    length = exp.Length,
                    reason = "This word looks different."
                };
            }

            if (usTokens.Count < exTokens.Count)
            {
                var exp = exTokens[usTokens.Count].raw;
                return new
                {
                    index = usTokens.Count,
                    preview = exp[..Math.Min(2, exp.Length)],
                    length = exp.Length,
                    reason = "You might be missing the next word."
                };
            }

            return new
            {
                index = Math.Max(0, exTokens.Count - 1),
                preview = "",
                length = 0,
                reason = "There are extra word(s) at the end."
            };
        }
    }
}
