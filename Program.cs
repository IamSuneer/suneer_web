using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;
using suneer_web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Data ─────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Auth services ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

// ── Email (SendGrid; logs code to console when API key is absent in dev) ──────
builder.Services.AddScoped<IEmailService, EmailService>();

// ── Cookie authentication ─────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/admin-suneer/page";
        options.LogoutPath        = "/admin/logout";
        options.AccessDeniedPath  = "/admin/denied";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name       = ".suneer.admin";
        options.Cookie.HttpOnly   = true;
        options.Cookie.SameSite   = SameSiteMode.Strict;
        // Always in production, SameAsRequest in development so HTTP still works locally
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
    });

// ── Response compression (bandwidth) ─────────────────────────────────────────
builder.Services.AddResponseCaching();

var app = builder.Build();

// ── Migrations + seed ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>();
    DbInitializer.Initialize(db, hasher);
}

// ── Error handling ────────────────────────────────────────────────────────────
// Must come first so it wraps the entire pipeline
app.UseExceptionHandler("/error/500");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();   // 30-day HSTS; browsers enforce HTTPS after first visit
}

// Friendly pages for 404 / 401 / etc.
app.UseStatusCodePagesWithReExecute("/error/{0}");

// ── Security headers ──────────────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options",  "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options",          "SAMEORIGIN");
    ctx.Response.Headers.Append("Referrer-Policy",          "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("Permissions-Policy",       "camera=(), microphone=(), geolocation=()");
    // X-XSS-Protection is deprecated — set to 0 to prevent legacy browser quirks
    ctx.Response.Headers.Append("X-XSS-Protection", "0");
    await next();
});

// ── Standard pipeline ─────────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // must precede Authorization
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
