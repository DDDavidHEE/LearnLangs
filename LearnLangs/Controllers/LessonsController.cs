using System;
using System.Linq;
using System.Threading.Tasks;
using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Controllers
{
    [Authorize] // <-- require login for all lesson pages
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
                .ToListAsync();

            return View(lessons);
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id, int? courseId)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null) return NotFound();

            ViewBag.CourseId = courseId ?? lesson.CourseId;
            return View(lesson);
        }

        // POST: Lessons/CompleteLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);

            // Find the lesson by its ID
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            // Add XP for completing this lesson (10 XP for simplicity)
            user.TotalXP += 10;

            // Set the user's last active date to today
            user.LastActiveDate = DateTime.Now;

            // Check if the user has completed lessons on consecutive days
            if (user.LastActiveDate.HasValue && user.LastActiveDate.Value.Date == DateTime.Now.Date.AddDays(-1))
            {
                // Increase streak if it's consecutive days
                user.CurrentStreak++;
            }
            else
            {
                // Reset streak if the user skipped a day
                user.CurrentStreak = 1;
            }

            // Check for milestone streak rewards
            if (user.CurrentStreak == 7 && !user.Has7DayStreakReward)
            {
                user.TotalXP += 50;  // Reward for 7-day streak
                user.Has7DayStreakReward = true;  // Mark the reward as given
            }
            else if (user.CurrentStreak == 30 && !user.Has30DayStreakReward)
            {
                user.TotalXP += 100;  // Reward for 30-day streak
                user.Has30DayStreakReward = true;  // Mark the reward as given
            }

            // Save the updated user data
            await _userManager.UpdateAsync(user);

            // Redirect to the lessons index for the course
            return RedirectToAction("Index", new { courseId = lesson.CourseId });
        }




        // GET: Lessons/Create
        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseId = courseId;

            // preselect course if provided
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

            // return to the lessons list for this course
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
