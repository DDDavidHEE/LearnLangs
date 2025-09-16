using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearnLangs.ViewModels;

namespace LearnLangs.Controllers
{
    [Authorize] // yêu cầu đăng nhập cho toàn bộ trang Lessons
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Lessons?courseId=1
        public async Task<IActionResult> Index(int? courseId)
        {
            if (courseId == null) return NotFound();

            ViewBag.CourseId = courseId;

            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderIndex)
                .AsNoTracking()
                .ToListAsync();

            return View(lessons);
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id, int? courseId)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null) return NotFound();

            ViewBag.CourseId = courseId ?? lesson.CourseId;
            return View(lesson);
        }

        // GET: Lessons/TakeQuiz?lessonId=123
        [HttpGet]
        public async Task<IActionResult> TakeQuiz(int lessonId)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return NotFound();

            var questions = await _context.Questions
                .AsNoTracking()
                .Where(q => q.LessonId == lessonId)
                .OrderBy(q => q.Id)
                .ToListAsync();

            var vm = new TakeQuizVM
            {
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                Questions = questions.Select(q => new QuizQuestionVM
                {
                    QuestionId = q.Id,
                    Prompt = q.Prompt,
                    IsMultipleChoice = q.IsMultipleChoice,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD
                }).ToList()
            };

            return View(vm);
        }

        // POST: Lessons/TakeQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeQuiz(TakeQuizVM vm)
        {
            if (vm == null) return BadRequest();

            // Lấy bài học
            var lesson = await _context.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == vm.LessonId);
            if (lesson == null) return NotFound();

            // Nạp câu hỏi thật từ DB
            var dbQuestions = await _context.Questions
                .AsNoTracking()
                .Where(q => q.LessonId == vm.LessonId)
                .OrderBy(q => q.Id)
                .ToListAsync();

            // Map đáp án người dùng (tránh null)
            var answersById = (vm.Questions ?? new System.Collections.Generic.List<QuizQuestionVM>())
                .ToDictionary(x => x.QuestionId, x => (x.UserAnswer ?? "").Trim());

            // Chấm điểm
            int correct = 0;
            foreach (var q in dbQuestions)
            {
                answersById.TryGetValue(q.Id, out var ans);
                ans = (ans ?? "").Trim();

                if (q.IsMultipleChoice)
                {
                    if (!string.IsNullOrWhiteSpace(ans) &&
                        string.Equals(ans, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        correct++;
                    }
                }
                else
                {
                    var corr = (q.ShortAnswer ?? q.FillInTheBlankAnswer ?? "").Trim();
                    if (!string.IsNullOrEmpty(corr) &&
                        string.Equals(ans, corr, StringComparison.OrdinalIgnoreCase))
                    {
                        correct++;
                    }
                }
            }

            // ---- Chỉ pass khi đúng hết ----
            int total = dbQuestions.Count;
            bool passed = (total > 0 && correct == total);

            // Lấy user & bản ghi UserLesson
            var user = await _userManager.GetUserAsync(User);
            var userLesson = await _context.UserLessons
                .FirstOrDefaultAsync(ul => ul.UserId == user.Id && ul.LessonId == vm.LessonId);

            bool alreadyCompleted = userLesson?.IsCompleted == true;

            if (userLesson == null)
            {
                userLesson = new UserLesson
                {
                    UserId = user.Id,
                    LessonId = vm.LessonId
                };
                _context.UserLessons.Add(userLesson);
            }

            // Lưu điểm lần làm này
            userLesson.Score = correct;

            // Chỉ đánh dấu hoàn thành & thời điểm khi LẦN ĐẦU pass
            if (!alreadyCompleted && passed)
            {
                userLesson.IsCompleted = true;
                userLesson.CompletedOn = DateTime.UtcNow;
            }

            // Chỉ cộng XP khi lần này pass và trước đó chưa hoàn thành
            int xpEarned = (!alreadyCompleted && passed) ? lesson.XpReward : 0;

            if (xpEarned > 0)
            {
                var today = DateTime.UtcNow.Date;
                var last = user.LastActiveDate?.Date;

                if (last == today)
                {
                    // đã hoạt động hôm nay
                }
                else if (last == today.AddDays(-1))
                {
                    user.CurrentStreak += 1;
                }
                else
                {
                    user.CurrentStreak = 1;
                }

                user.LastActiveDate = today;
                user.TotalXP += xpEarned;

                // Thưởng streak
                if (user.CurrentStreak >= 7 && !user.Has7DayStreakReward)
                {
                    user.TotalXP += 50;
                    user.Has7DayStreakReward = true;
                }
                if (user.CurrentStreak >= 30 && !user.Has30DayStreakReward)
                {
                    user.TotalXP += 100;
                    user.Has30DayStreakReward = true;
                }

                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            // Trả view kết quả
            ViewBag.CourseId = lesson.CourseId;
            var resultVm = new QuizResultVM
            {
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                Correct = correct,
                Total = total,
                XpAwarded = xpEarned
            };

            return View("QuizResult", resultVm);
        }

        // POST: Lessons/CompleteLesson (đã vô hiệu hoá: chuyển sang làm quiz)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteLesson(int lessonId)
        {
            return RedirectToAction(nameof(TakeQuiz), new { lessonId });
        }

        // GET: Lessons/Create
        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseId = courseId;
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", courseId);
            return View();
        }

        // POST: Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,Title,OrderIndex,XpReward")] Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", lesson.CourseId);
                ViewBag.CourseId = lesson.CourseId;
                return View(lesson);
            }

            _context.Add(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", lesson.CourseId);
            ViewBag.CourseId = lesson.CourseId;
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,Title,OrderIndex,XpReward")] Lesson lesson)
        {
            if (id != lesson.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", lesson.CourseId);
                ViewBag.CourseId = lesson.CourseId;
                return View(lesson);
            }

            try
            {
                _context.Update(lesson);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Lessons.Any(e => e.Id == lesson.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null) return NotFound();

            ViewBag.CourseId = lesson.CourseId;
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                int courseId = lesson.CourseId;
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { courseId });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
