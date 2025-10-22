using LearnLangs.Data;
using LearnLangs.Models;

using LearnLangs.Hubs;                        // ChatHub (nếu còn dùng SignalR tính năng khác)
using LearnLangs.Options;                    // AzureTranslatorOptions, GeminiAiOptions
using LearnLangs.Services.Chat;              // IGeminiChatService, GeminiChatService
using LearnLangs.Services.Pronunciation;     // IGeminiPronunciationService, GeminiPronunciationService
using LearnLangs.Services.Translate;         // ITranslateService, AzureTranslatorService

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

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

// ================== MVC + APIs ==================
builder.Services.AddControllersWithViews();

// ================== SignalR (nếu vẫn dùng nơi khác) ==================
builder.Services.AddSignalR(); // không bắt buộc cho Conversation kiểu mới

// ================== Azure Translator ==================
builder.Services.Configure<AzureTranslatorOptions>(
    builder.Configuration.GetSection("AzureTranslator"));
builder.Services.AddHttpClient<ITranslateService, AzureTranslatorService>();

// ================== Gemini (Options dùng chung) ==================
builder.Services.Configure<GeminiAiOptions>(
    builder.Configuration.GetSection("GeminiAI"));

// Fallback & auto ApiVersion theo model
builder.Services.PostConfigure<GeminiAiOptions>(opt =>
{
    // Fallback API key nếu chưa có
    if (string.IsNullOrWhiteSpace(opt.ApiKey))
    {
        var k = builder.Configuration["davidheapikey"];
        if (!string.IsNullOrWhiteSpace(k)) opt.ApiKey = k;
    }

    // Suy luận ApiVersion nếu để trống
    if (string.IsNullOrWhiteSpace(opt.ApiVersion))
    {
        var m = (opt.Model ?? "").ToLowerInvariant();
        if (m.StartsWith("gemini-1.") || m.StartsWith("gemini-1-") || m.StartsWith("gemini-1_")
            || m.StartsWith("gemini-1.5") || m.StartsWith("gemini-1_5"))
        {
            opt.ApiVersion = "v1";
        }
        else
        {
            // 2.x (2.0, 2.5, …)
            opt.ApiVersion = "v1beta";
        }
    }

    // Chuẩn hóa endpoint
    if (string.IsNullOrWhiteSpace(opt.Endpoint))
        opt.Endpoint = "https://generativelanguage.googleapis.com";
});

// ---- Gemini: Pronunciation (nếu bạn còn dùng) ----
builder.Services.AddHttpClient<IGeminiPronunciationService, GeminiPronunciationService>(http =>
{
    http.Timeout = TimeSpan.FromSeconds(60);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ---- Gemini: Chat (Conversation – gọi trực tiếp API, không SignalR) ----
builder.Services.AddHttpClient<IGeminiChatService, GeminiChatService>(http =>
{
    http.Timeout = TimeSpan.FromSeconds(60);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// ==== Seed data & admin ====
await SeedData.EnsureSeededAsync(app.Services);
await IdentitySeed.EnsureAdminAsync(app.Services);

// ===== Log quick check (không lộ key) =====
var transOpts = app.Services.GetRequiredService<IOptions<AzureTranslatorOptions>>().Value;
app.Logger.LogInformation("[AzureTranslator] Region={Region}, Endpoint={Endpoint}, KeySet={KeySet}",
    transOpts.Region, transOpts.Endpoint, !string.IsNullOrEmpty(transOpts.SubscriptionKey));

var geminiOpts = app.Services.GetRequiredService<IOptions<GeminiAiOptions>>().Value;
app.Logger.LogInformation("[GeminiAI] Model={Model}, Endpoint={Endpoint}, ApiVersion={ApiVersion}, KeySet={KeySet}",
    geminiOpts.Model, geminiOpts.Endpoint, geminiOpts.ApiVersion, !string.IsNullOrEmpty(geminiOpts.ApiKey));

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

// ===== Hubs (SignalR) – nếu còn dùng tính năng cũ =====
app.MapHub<ChatHub>("/hubs/chat");

// ===== Routes =====
// Route RIÊNG cho Conversation: /Conversation (đi tới ConversationController)
app.MapControllerRoute(
    name: "conversation",
    pattern: "Conversation/{action=Index}/{id?}",
    defaults: new { controller = "Conversation" }
);

// Route mặc định của toàn site
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Quan trọng: bật attribute-routed APIs (ví dụ /api/conversation/send)
app.MapControllers();

app.MapRazorPages();

app.Run();
