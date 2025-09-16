using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Authorization;

namespace LearnLangs.Controllers
{
    // YÊU CẦU đăng nhập cho mặc định tất cả action trong controller này
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Cho phép xem công khai (không cần đăng nhập) nếu bạn muốn
        [AllowAnonymous]
        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            return View(courses);
        }

        // Cho phép xem công khai (không cần đăng nhập) nếu bạn muốn
        [AllowAnonymous]
        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // GET: Courses/Create  (chỉ người đã đăng nhập do [Authorize] trên class)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create  (chỉ người đã đăng nhập)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Course course)
        {
            if (!ModelState.IsValid) return View(course);

            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/Edit/5  (chỉ người đã đăng nhập)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Courses/Edit/5  (chỉ người đã đăng nhập)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Course course)
        {
            if (id != course.Id) return NotFound();
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

        // GET: Courses/Delete/5  (chỉ người đã đăng nhập)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Courses/Delete/5  (chỉ người đã đăng nhập)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id) =>
            _context.Courses.Any(e => e.Id == id);
    }
}
