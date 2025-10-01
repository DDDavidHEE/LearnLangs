using LearnLangs.Services.Translate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnLangs.Controllers
{
    [Authorize] // muốn public thì bỏ dòng này
    public class TranslateController : Controller
    {
        private readonly ITranslateService _svc;

        public TranslateController(ITranslateService svc) => _svc = svc;

        // Danh sách ngôn ngữ phổ biến (mã BCP-47 dùng bởi Azure)
        private static readonly (string Code, string Name)[] Langs = new[]
        {
            ("auto", "Auto detect"),
            ("en", "English"),
            ("vi", "Vietnamese"),
            ("zh-Hans", "Chinese (Simplified)"),
            ("zh-Hant", "Chinese (Traditional)"),
            ("ja", "Japanese"),
            ("ko", "Korean"),
            ("fr", "French"),
            ("de", "German"),
            ("es", "Spanish"),
            ("pt", "Portuguese"),
            ("ru", "Russian"),
            ("id", "Indonesian"),
            ("th", "Thai"),
            ("it", "Italian"),
            ("tr", "Turkish"),
        };

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Langs = Langs;
            ViewBag.InputText = ""; // quan trọng: để view không đọc Request.Form trên GET
            ViewBag.OutputText = "";
            ViewBag.Detected = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string inputText, string from, string to)
        {
            ViewBag.Langs = Langs;
            ViewBag.InputText = inputText ?? ""; // trả lại input cho view

            from ??= "auto";
            to ??= "en";

            if (string.IsNullOrWhiteSpace(inputText))
            {
                ModelState.AddModelError(nameof(inputText), "Please enter text.");
                ViewBag.OutputText = "";
                ViewBag.Detected = "";
                return View();
            }

            try
            {
                var result = await _svc.TranslateAsync(inputText, from, to);
                ViewBag.OutputText = result.TranslatedText;
                ViewBag.Detected = result.DetectedLanguage;
            }
            catch (Exception ex)
            {
                // hiển thị lỗi nhẹ nhàng cho người dùng
                ModelState.AddModelError(string.Empty, $"Translate failed: {ex.Message}");
                ViewBag.OutputText = "";
                ViewBag.Detected = "";
            }

            return View();
        }
    }
}
