using LearnLangs.Data;
using LearnLangs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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
        public async Task<IActionResult> Index()
        {
            // Get the top users ordered by XP
            var topUsers = await _context.Users
                                         .OrderByDescending(u => u.TotalXP)  // Order by XP
                                         .Take(10)  // Limit to top 10 users
                                         .ToListAsync();

            return View(topUsers);
        }
    }
}
