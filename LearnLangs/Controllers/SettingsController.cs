using LearnLangs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // CookieOptions

namespace LearnLangs.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private static readonly (string Code, string Name)[] TranslateLangs = new[]
        {
            ("auto","Auto detect"),
            ("en","English"),
            ("vi","Vietnamese"),
            ("zh-Hans","Chinese (Simplified)"),
            ("zh-Hant","Chinese (Traditional)"),
            ("ja","Japanese"),
            ("ko","Korean"),
            ("fr","French"),
            ("de","German"),
            ("es","Spanish"),
            ("pt","Portuguese"),
            ("ru","Russian"),
            ("id","Indonesian"),
            ("th","Thai"),
            ("it","Italian"),
            ("tr","Turkish"),
        };

        private static readonly (string Code, string Name)[] PronunLangs = new[]
        {
            ("en-US","English (US)"),
            ("en-GB","English (UK)"),
            ("vi-VN","Vietnamese"),
            ("zh-CN","Chinese (Mainland)"),
            ("ja-JP","Japanese"),
            ("ko-KR","Korean"),
            ("fr-FR","French"),
            ("de-DE","German"),
            ("es-ES","Spanish")
        };

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new SettingsViewModel
            {
                PreferredUiTheme = user.PreferredUiTheme ?? "light",
                From = user.DefaultTranslateFrom ?? "auto",
                To = user.DefaultTranslateTo ?? "en",
                PronunciationLang = user.DefaultPronunciationLang ?? "en-US",
                ShowPronunciationRawJson = user.ShowPronunciationRawJson
            };

            ViewBag.TranslateLangs = TranslateLangs;
            ViewBag.PronunLangs = PronunLangs;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                ViewBag.TranslateLangs = TranslateLangs;
                ViewBag.PronunLangs = PronunLangs;
                return View(vm);
            }

            // --- sanitize theme ---
            var theme = (vm.PreferredUiTheme ?? "light").ToLowerInvariant();
            if (theme != "dark" && theme != "light") theme = "light";

            // Lưu DB
            user.PreferredUiTheme = theme;
            user.DefaultTranslateFrom = vm.From;
            user.DefaultTranslateTo = vm.To;
            user.DefaultPronunciationLang = vm.PronunciationLang;
            user.ShowPronunciationRawJson = vm.ShowPronunciationRawJson;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                ViewBag.TranslateLangs = TranslateLangs;
                ViewBag.PronunLangs = PronunLangs;
                return View(vm);
            }

            // Đặt cookie để _Layout đọc và đổi theme ngay
            Response.Cookies.Append(
                "ui-theme",
                theme,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    HttpOnly = false,
                    Secure = Request.IsHttps
                });

            TempData["SettingsSaved"] = true;
            return RedirectToAction(nameof(Index));
        }
    }
}
