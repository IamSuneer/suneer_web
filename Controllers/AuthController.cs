using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;
using suneer_web.Models.ViewModels;

namespace suneer_web.Controllers;

[Route("admin-suneer")]
public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AdminUser> _hasher;

    public AuthController(AppDbContext db, IPasswordHasher<AdminUser> hasher)
    {
        _db     = db;
        _hasher = hasher;
    }

    // GET /admin-suneer/page  ← this is the LoginPath in cookie config
    [HttpGet("page")]
    public IActionResult Page()
    {
        // Already logged in → go straight to dashboard
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        return View();
    }

    // POST /admin/login  ← form action target (absolute route avoids /admin-suneer prefix)
    [HttpPost("/admin/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Page", model);

        var admin = await _db.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == model.Email);

        if (admin is null)
        {
            // Generic error — do NOT reveal whether the email exists
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("Page", model);
        }

        var result = _hasher.VerifyHashedPassword(admin, admin.PasswordHash, model.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("Page", model);
        }

        // Build claims identity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name,  admin.Email),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role,  "Admin")
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc   = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps);

        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    // POST /admin/logout
    [HttpPost("/admin/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Page", "Auth");
    }

    // GET /admin/denied  ← AccessDeniedPath in cookie config
    [HttpGet("/admin/denied")]
    public IActionResult Denied() => View();
}
