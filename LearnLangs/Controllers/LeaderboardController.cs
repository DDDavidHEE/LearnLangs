using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using X.PagedList;
using X.PagedList.Extensions;

namespace LearnLangs.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaderboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leaderboard
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;

            // Apply pagination directly on IQueryable
            var topUsers = _context.Users
                .OrderByDescending(u => u.TotalXP);  // IQueryable (no ToListAsync)

            // Convert to a List first, and then paginate
            var usersList = await topUsers.ToListAsync(); // Fetch data from DB

            // Apply pagination on the List
            var pagedUsers = usersList.ToPagedList(page, pageSize);  // Pagination logic

            return View(pagedUsers);  // Return paginated users to view
        }
    }
}
