using Microsoft.AspNetCore.Mvc;
using LearnLangs.Models;
using LearnLangs.Services.Pronunciation;

namespace LearnLangs.Controllers
{
    public class PronunciationController : Controller
    {
        private readonly IGeminiPronunciationService _gemini;

        public PronunciationController(IGeminiPronunciationService gemini)
        {
            _gemini = gemini;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new PronunciationInputModel { ReferenceText = "Hello world" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(PronunciationInputModel model)
        {
            if (model.AudioFile == null || model.AudioFile.Length == 0)
            {
                ModelState.AddModelError("", "No audio uploaded.");
                return View("Index", model);
            }

            using var ms = new MemoryStream();
            await model.AudioFile.CopyToAsync(ms);
            var audioBytes = ms.ToArray();

            // Dùng ContentType thật của file (webm/ogg/wav)
            var mime = string.IsNullOrWhiteSpace(model.AudioFile.ContentType) ? "audio/webm" : model.AudioFile.ContentType;

            var result = await _gemini.AssessWithGeminiAsync(
                audioBytes,
                model.ReferenceText ?? "Hello world",
                mime
            );

            // (Tuỳ chọn) Bạn có thể đính kèm model.SpokenText vào ViewData để hiển thị cùng kết quả
            ViewData["SpokenText"] = model.SpokenText;

            return View("AnalysisResult", result);
        }
    }
}
