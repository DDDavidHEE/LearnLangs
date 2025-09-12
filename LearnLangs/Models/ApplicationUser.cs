using Microsoft.AspNetCore.Identity;
using System;

namespace LearnLangs.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int TotalXP { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public DateTime? LastActiveDate { get; set; }
    }
}
