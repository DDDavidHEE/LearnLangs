using LearnLangs.Data;
using LearnLangs.Models.Flashcards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Controllers
{
    [Authorize]
    public class FlashcardsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public FlashcardsController(ApplicationDbContext db) { _db = db; }

        // /Flashcards?mode=basic|ielts|hsk
        [HttpGet]
        public async Task<IActionResult> Index(string mode = "basic")
        {
            mode = (mode ?? "basic").ToLower().Trim();

            var decks = await _db.FlashcardDecks
                .Where(d => d.Mode == mode)
                .OrderBy(d => d.OrderIndex)
                .Select(d => new FlashDeckVM
                {
                    Id = d.Id,
                    Title = d.Title,
                    CoverUrl = d.CoverUrl,
                    Count = d.Cards.Count(),   // EF Core translate thành subquery
                    Mode = d.Mode
                })
                .ToListAsync();

            ViewBag.Mode = mode;
            return View(decks);
        }

        // /Flashcards/Play/5?part=1
        [HttpGet]
        public async Task<IActionResult> Play(int id, int part = 1)
        {
            const int pageSize = 20;

            var deck = await _db.FlashcardDecks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deck == null) return NotFound();

            var cards = deck.Cards.OrderBy(c => c.OrderIndex).ToList();
            var totalCount = cards.Count;
            var totalParts = (int)Math.Ceiling(totalCount / (double)pageSize);
            part = Math.Max(1, Math.Min(totalParts == 0 ? 1 : totalParts, part));

            var items = cards
                .Skip((part - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new FlashCardVM
                {
                    Id = c.Id,
                    Word = c.FrontWord,
                    Pos = c.Pos,
                    Ipa = c.Ipa,
                    Phonetic = c.Phonetic,
                    MeaningVi = c.BackMeaningVi,
                    ExampleEn = c.ExampleEn,
                    ExampleVi = c.ExampleVi,
                    ImageUrl = c.ImageUrl
                })
                .ToList();

            var vm = new PlayDeckVM
            {
                DeckId = deck.Id,
                DeckTitle = deck.Title,
                CoverUrl = deck.CoverUrl,
                Mode = deck.Mode,
                Part = part,
                TotalParts = Math.Max(1, totalParts),
                Cards = items,
                TotalCards = totalCount
            };

            return View(vm);
        }

        // tạm thời
        [HttpPost]
        public IActionResult MarkKnown([FromForm] int cardId, [FromForm] bool known)
        {
            return Json(new { ok = true });
        }
    }
}
