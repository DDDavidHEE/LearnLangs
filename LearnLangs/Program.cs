using LearnLangs.Data;
using LearnLangs.Models;

using LearnLangs.Options;                      // AzureSpeechOptions, AzureTranslatorOptions
using LearnLangs.Services.Pronunciation;       // IPronunciationAssessmentService
using LearnLangs.Services.Translate;           // ITranslateService, AzureTranslatorService

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;            // IOptions<T>

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

// ================== Azure Speech (Pronunciation) ==================
builder.Services.Configure<AzureSpeechOptions>(
    builder.Configuration.GetSection("AzureSpeech"));
builder.Services.AddScoped<IPronunciationAssessmentService, PronunciationAssessmentService>();

// ================== Azure Translator (Text Translate) ==================
builder.Services.Configure<AzureTranslatorOptions>(
    builder.Configuration.GetSection("AzureTranslator"));
builder.Services.AddHttpClient<ITranslateService, AzureTranslatorService>();

var app = builder.Build();

// ==== Seed data & admin ====
await SeedData.EnsureSeededAsync(app.Services);
await IdentitySeed.EnsureAdminAsync(app.Services);

// ===== Log quick check (không lộ key) =====
var speechOpts = app.Services.GetRequiredService<IOptions<AzureSpeechOptions>>().Value;
app.Logger.LogInformation("[AzureSpeech] Region={Region}, Lang={Lang}, KeySet={KeySet}",
    speechOpts.Region, speechOpts.RecognitionLanguage, !string.IsNullOrEmpty(speechOpts.SubscriptionKey));

var transOpts = app.Services.GetRequiredService<IOptions<AzureTranslatorOptions>>().Value;
app.Logger.LogInformation("[AzureTranslator] Region={Region}, Endpoint={Endpoint}, KeySet={KeySet}",
    transOpts.Region, transOpts.Endpoint, !string.IsNullOrEmpty(transOpts.SubscriptionKey));

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
