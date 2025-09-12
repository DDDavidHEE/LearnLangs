using System;
using System.Linq;
using System.Threading.Tasks;
using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LearnLangs.Controllers
{
    [Authorize] // <-- require login for all lesson pages
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonsController(ApplicationDbContext context)
        {
            _context = context;
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
