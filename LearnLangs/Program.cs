using LearnLangs.Data;
using LearnLangs.Models;
using LearnLangs.Hubs;
using LearnLangs.Options;
using LearnLangs.Services.Chat;
using LearnLangs.Services.Pronunciation;
using LearnLangs.Services.Translate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

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

// ==================== MVC + APIs ====================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ==================== SIGNALR ====================
builder.Services.AddSignalR();

// ==================== AZURE TRANSLATOR ====================
builder.Services.Configure<AzureTranslatorOptions>(
    builder.Configuration.GetSection("AzureTranslator"));

builder.Services.AddHttpClient<ITranslateService, AzureTranslatorService>((serviceProvider, http) =>
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
    // Fallback API key from multiple sources
    if (string.IsNullOrWhiteSpace(opt.ApiKey))
    {
        // Try 1: davidheapikey (legacy)
        var key = builder.Configuration["davidheapikey"];

        // Try 2: GeminiAI:ApiKey (preferred)
        if (string.IsNullOrWhiteSpace(key))
        {
            key = builder.Configuration["GeminiAI:ApiKey"];
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            opt.ApiKey = key;
        }
    }

    // Auto-detect ApiVersion based on model
    if (string.IsNullOrWhiteSpace(opt.ApiVersion))
    {
        var model = (opt.Model ?? "").ToLowerInvariant();

        // Gemini 1.x uses v1
        if (model.StartsWith("gemini-1.") ||
            model.StartsWith("gemini-1-") ||
            model.StartsWith("gemini-1_") ||
            model.Contains("1.5"))
        {
            opt.ApiVersion = "v1";
        }
        else
        {
            // Gemini 2.x uses v1beta
            opt.ApiVersion = "v1beta";
        }
    }

    // Default endpoint
    if (string.IsNullOrWhiteSpace(opt.Endpoint))
    {
        opt.Endpoint = "https://generativelanguage.googleapis.com";
    }
});

// ==================== GEMINI: PRONUNCIATION ====================
builder.Services.AddHttpClient<IGeminiPronunciationService, GeminiPronunciationService>((serviceProvider, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(60);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ==================== GEMINI: CHAT (CONVERSATION) ====================
builder.Services.AddHttpClient<IGeminiChatService, GeminiChatService>((serviceProvider, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(60);
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== SEED DATA ====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.EnsureSeededAsync(scope.ServiceProvider);
        await IdentitySeed.EnsureAdminAsync(scope.ServiceProvider);
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

// WARNING: Log if API key is missing
if (string.IsNullOrEmpty(geminiOpts.ApiKey))
{
    app.Logger.LogWarning(
        "⚠️ [GeminiAI] API Key is NOT SET! " +
        "Set it using: dotnet user-secrets set \"GeminiAI:ApiKey\" \"YOUR_KEY\""
    );
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
app.UseAuthorization();

// ==================== ROUTING ====================

// SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat");

// MVC Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API Controllers (attribute-routed like [Route("api/conversation")])
app.MapControllers();

// Razor Pages
app.MapRazorPages();

// ==================== RUN ====================
app.Logger.LogInformation("🚀 Application starting...");
app.Run();