using System;
using System.Threading.Tasks;
using LearnLangs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LearnLangs.Data
{
    public static class IdentitySeed
    {
        public static async Task EnsureAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            const string roleName = "Admin";
            if (!await roles.RoleExistsAsync(roleName))
            {
                await roles.CreateAsync(new IdentityRole(roleName));
            }

            // Lấy thông tin admin từ cấu hình (nếu không có dùng mặc định)
            var email = config["Seed:AdminEmail"] ?? "admin@example.com";
            var password = config["Seed:AdminPassword"] ?? "Admin!12345";

            var admin = await users.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var created = await users.CreateAsync(admin, password);
                if (!created.Succeeded) return; // đơn giản bỏ qua nếu tạo lỗi
            }

            if (!await users.IsInRoleAsync(admin, roleName))
            {
                await users.AddToRoleAsync(admin, roleName);
            }
        }
    }
}
