using LearnLangs.Data;
using LearnLangs.Models;
using LearnLangs.Hubs;
using LearnLangs.Options;
using LearnLangs.Services.Chat;
using LearnLangs.Services.Pronunciation;
using LearnLangs.Services.Translate;
using LearnLangs.Services.Games;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http; // 👈 for CookieSecurePolicy, SameSiteMode

var builder = WebApplication.CreateBuilder(args);

// ==================== LOGGING ====================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ==================== DATABASE ====================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ==================== IDENTITY ====================
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ✅ Cookie đăng nhập – tránh “mất phiên” & chỉ gửi qua HTTPS
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "LearnLangs.Identity";
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // chỉ gửi trên HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;              // đủ cho điều hướng nội bộ
});

// ==================== MVC + APIs ====================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ==================== SIGNALR ====================
builder.Services.AddSignalR();

// ==================== AZURE TRANSLATOR ====================
builder.Services.Configure<AzureTranslatorOptions>(
    builder.Configuration.GetSection("AzureTranslator"));

builder.Services.AddHttpClient<ITranslateService, AzureTranslatorService>((sp, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(30);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ==================== GEMINI AI OPTIONS ====================
builder.Services.Configure<GeminiAiOptions>(
    builder.Configuration.GetSection("GeminiAI"));

// Post-configure: Fallback API key & ApiVersion
builder.Services.PostConfigure<GeminiAiOptions>(opt =>
{
    if (string.IsNullOrWhiteSpace(opt.ApiKey))
    {
        var key = builder.Configuration["davidheapikey"];
        if (string.IsNullOrWhiteSpace(key))
            key = builder.Configuration["GeminiAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(key))
            opt.ApiKey = key;
    }

    if (string.IsNullOrWhiteSpace(opt.ApiVersion))
    {
        var model = (opt.Model ?? "").ToLowerInvariant();
        if (model.StartsWith("gemini-1.") || model.StartsWith("gemini-1-") || model.StartsWith("gemini-1_") || model.Contains("1.5"))
            opt.ApiVersion = "v1";
        else
            opt.ApiVersion = "v1beta";
    }

    if (string.IsNullOrWhiteSpace(opt.Endpoint))
        opt.Endpoint = "https://generativelanguage.googleapis.com";
});

// ==================== GEMINI: PRONUNCIATION ====================
builder.Services.AddHttpClient<IGeminiPronunciationService, GeminiPronunciationService>((sp, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(60);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ==================== GEMINI: CHAT (CONVERSATION) ====================
builder.Services.AddHttpClient<IGeminiChatService, GeminiChatService>((sp, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(60);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ==================== GAMES/EXAMS DI ====================
builder.Services.AddScoped<GameService>();   // giữ nguyên

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== SEED DATA ====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.EnsureSeededAsync(scope.ServiceProvider);
        await IdentitySeed.EnsureAdminAsync(scope.ServiceProvider);

        // nếu có Seed cho Games/Exams
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await LearnLangs.Data.GameSeed.SeedAsync(db);
        await LearnLangs.Data.ExamSeed.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred while seeding database");
    }
}

// ==================== LOG CONFIGURATION CHECK ====================
var translatorOpts = app.Services.GetRequiredService<IOptions<AzureTranslatorOptions>>().Value;
app.Logger.LogInformation(
    "[AzureTranslator] Region={Region}, Endpoint={Endpoint}, KeySet={KeySet}",
    translatorOpts.Region,
    translatorOpts.Endpoint,
    !string.IsNullOrEmpty(translatorOpts.SubscriptionKey)
);

var geminiOpts = app.Services.GetRequiredService<IOptions<GeminiAiOptions>>().Value;
app.Logger.LogInformation(
    "[GeminiAI] Model={Model}, Endpoint={Endpoint}, ApiVersion={ApiVersion}, KeySet={KeySet}",
    geminiOpts.Model,
    geminiOpts.Endpoint,
    geminiOpts.ApiVersion,
    !string.IsNullOrEmpty(geminiOpts.ApiKey)
);

if (string.IsNullOrEmpty(geminiOpts.ApiKey))
{
    app.Logger.LogWarning("⚠️ [GeminiAI] API Key is NOT SET! Set it using: dotnet user-secrets set \"GeminiAI:ApiKey\" \"YOUR_KEY\"");
}

// ==================== MIDDLEWARE PIPELINE ====================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// ✅ ĐÃ ĐĂNG NHẬP mà vào /Login hoặc /Register → chuyển hướng về trang chủ (hoặc returnUrl)
app.Use(async (ctx, next) =>
{
    if (ctx.User?.Identity?.IsAuthenticated == true)
    {
        var path = ctx.Request.Path.Value?.ToLowerInvariant();
        if (path == "/identity/account/login" || path == "/identity/account/register")
        {
            var ret = ctx.Request.Query["returnUrl"].ToString();
            // tránh open-redirect: chỉ cho phép đường dẫn tương đối nội bộ
            var isLocal = !string.IsNullOrWhiteSpace(ret) && ret.StartsWith("/") && !ret.StartsWith("//") && !ret.StartsWith("/\\");
            ctx.Response.Redirect(isLocal ? ret : "/");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

// ==================== ROUTING ====================
app.MapHub<ChatHub>("/hubs/chat");

// MVC Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API Controllers (attribute-routed)
app.MapControllers();

// Razor Pages (Identity UI)
app.MapRazorPages();

// ==================== RUN ====================
app.Logger.LogInformation("🚀 Application starting...");
app.Run();
