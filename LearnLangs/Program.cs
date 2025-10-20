using LearnLangs.Data;
using LearnLangs.Models;

using LearnLangs.Hubs;                        // ChatHub
using LearnLangs.Options;                    // AzureTranslatorOptions, GeminiAiOptions
using LearnLangs.Services.Chat;              // IGeminiChatService, GeminiChatService
using LearnLangs.Services.Pronunciation;     // IGeminiPronunciationService, GeminiPronunciationService
using LearnLangs.Services.Translate;         // ITranslateService, AzureTranslatorService

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ===== Logging (dev) =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ================== DB ==================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ================== Identity ==================
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // easier for local dev
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ================== MVC ==================
builder.Services.AddControllersWithViews();

// ================== SignalR (Realtime chat) ==================
builder.Services.AddSignalR();

// ================== Azure Translator ==================
builder.Services.Configure<AzureTranslatorOptions>(
    builder.Configuration.GetSection("AzureTranslator"));
builder.Services.AddHttpClient<ITranslateService, AzureTranslatorService>();

// ================== Gemini (Options dùng chung) ==================
builder.Services.Configure<GeminiAiOptions>(
    builder.Configuration.GetSection("GeminiAI"));

// ---- Gemini: Pronunciation (nếu bạn còn dùng) ----
builder.Services.AddHttpClient<IGeminiPronunciationService, GeminiPronunciationService>();

// ---- Gemini: Chat (hội thoại AI) ----
builder.Services.AddHttpClient<IGeminiChatService, GeminiChatService>();

var app = builder.Build();

// ==== Seed data & admin ====
await SeedData.EnsureSeededAsync(app.Services);
await IdentitySeed.EnsureAdminAsync(app.Services);

// ===== Log quick check (không lộ key) =====
var transOpts = app.Services.GetRequiredService<IOptions<AzureTranslatorOptions>>().Value;
app.Logger.LogInformation("[AzureTranslator] Region={Region}, Endpoint={Endpoint}, KeySet={KeySet}",
    transOpts.Region, transOpts.Endpoint, !string.IsNullOrEmpty(transOpts.SubscriptionKey));

var geminiOpts = app.Services.GetRequiredService<IOptions<GeminiAiOptions>>().Value;
app.Logger.LogInformation("[GeminiAI] Model={Model}, Endpoint={Endpoint}, KeySet={KeySet}",
    geminiOpts.Model, geminiOpts.Endpoint, !string.IsNullOrEmpty(geminiOpts.ApiKey));

// ================== Pipeline ==================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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
app.UseAuthorization();

// ===== Hubs (SignalR) =====
app.MapHub<ChatHub>("/hubs/chat");

// ===== Routes =====
// Route RIÊNG cho Conversation: /Conversation (controller = Chat)
app.MapControllerRoute(
    name: "conversation",
    pattern: "Conversation/{action=Index}/{id?}",
    defaults: new { controller = "Chat" }
);

// Route mặc định của toàn site (KHÔNG phải Chat)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
