// LearnLangs/Controllers/PronunciationController.cs
using System.Threading;
using System.Threading.Tasks;
using LearnLangs.Models;
using LearnLangs.Services.Pronunciation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnLangs.Controllers
{
    [Authorize]
    public class PronunciationController : Controller
    {
        private readonly IPronunciationAssessmentService _svc;

        public PronunciationController(IPronunciationAssessmentService svc)
        {
            _svc = svc;
        }

        // GET: /Pronunciation
        [HttpGet]
        public IActionResult Index(string? text = null)
        {
            var model = new PronunciationInputModel
            {
                ReferenceText = text ?? string.Empty
            };
            return View(model);
        }

        // POST: /Pronunciation/Assess
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assess(PronunciationInputModel input, CancellationToken ct)
        {
            // Kiểm tra dữ liệu form
            if (string.IsNullOrWhiteSpace(input?.ReferenceText))
                ModelState.AddModelError(nameof(input.ReferenceText), "Vui lòng nhập câu cần đọc.");

            if (input?.AudioFile is null || input.AudioFile.Length == 0)
                ModelState.AddModelError(nameof(input.AudioFile), "Vui lòng chọn tệp âm thanh.");

            // (Không bắt buộc) giới hạn kích thước upload ~20MB
            if (input?.AudioFile is not null && input.AudioFile.Length > 20 * 1024 * 1024)
                ModelState.AddModelError(nameof(input.AudioFile), "Tệp quá lớn (giới hạn 20MB).");

            if (!ModelState.IsValid)
                return View("Index", input);

            // Gọi service chấm điểm
            var result = await _svc.AssessAsync(input.ReferenceText, input.AudioFile, ct);

            // Trả về view hiển thị điểm
            return View("Result", result);
        }
    }
}
