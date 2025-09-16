using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Authorization;

namespace LearnLangs.Controllers
{
    // Mặc định yêu cầu đăng nhập (các action đọc sẽ mở lại bằng [AllowAnonymous])
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================= READ-ONLY (public) =========================

        // GET: Courses (ai cũng xem được)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            return View(courses);
        }

        // GET: Courses/Details/5 (ai cũng xem được)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Lessons)              // để view hiển thị danh sách bài
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            // Sắp xếp Lessons nếu view sử dụng Model.Lessons
            if (course.Lessons != null)
            {
                course.Lessons = course.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .ThenBy(l => l.Id)
                    .ToList();
            }

            return View(course);
        }

        // ========================= CREATE (Admin only) =========================

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description")] Course course)
        {
            // Chống trùng tên
            if (await _context.Courses.AsNoTracking().AnyAsync(c => c.Name == course.Name))
            {
                ModelState.AddModelError(nameof(Course.Name), "Course name already exists.");
            }

            if (!ModelState.IsValid) return View(course);

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ========================= EDIT (Admin only) =========================

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Course course)
        {
            if (id != course.Id) return NotFound();

            // Chống trùng tên (trừ chính nó)
            if (await _context.Courses
                    .AsNoTracking()
                    .AnyAsync(c => c.Id != id && c.Name == course.Name))
            {
                ModelState.AddModelError(nameof(Course.Name), "Course name already exists.");
            }

            if (!ModelState.IsValid) return View(course);

            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================= DELETE (Admin only) =========================

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Lessons)      // kiểm tra còn lesson để cảnh báo
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            ViewBag.HasLessons = course.Lessons != null && course.Lessons.Any();
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return RedirectToAction(nameof(Index));

            // Không cho xoá nếu còn lesson (tránh mất dữ liệu / lỗi FK)
            if (course.Lessons != null && course.Lessons.Any())
            {
                TempData["CourseMsg"] = "Cannot delete course that still has lessons. Remove lessons first.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ========================= Helpers =========================

        private bool CourseExists(int id) =>
            _context.Courses.Any(e => e.Id == id);
    }
}
