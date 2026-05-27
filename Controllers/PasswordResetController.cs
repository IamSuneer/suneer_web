using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;
using suneer_web.Models.ViewModels;
using suneer_web.Services;

namespace suneer_web.Controllers;

[Route("admin")]
public class PasswordResetController : Controller
{
    // All auth-related views live alongside the login view in Views/Auth/
    private const string ForgotView = "~/Views/Auth/ForgotPassword.cshtml";
    private const string VerifyView = "~/Views/Auth/VerifyCode.cshtml";
    private const string ResetView  = "~/Views/Auth/ResetPassword.cshtml";

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AdminUser> _hasher;
    private readonly IEmailService _email;
    private readonly ILogger<PasswordResetController> _logger;

    public PasswordResetController(
        AppDbContext db,
        IPasswordHasher<AdminUser> hasher,
        IEmailService email,
        ILogger<PasswordResetController> logger)
    {
        _db     = db;
        _hasher = hasher;
        _email  = email;
        _logger = logger;
    }

    // =========================================================================
    // STEP 1 — Forgot Password
    // =========================================================================

    // GET /admin/forgot-password
    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword()
        => View(ForgotView, new ForgotPasswordViewModel());

    // POST /admin/forgot-password
    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    [ActionName("ForgotPassword")]
    public async Task<IActionResult> ForgotPasswordPost(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(ForgotView, model);

        // Housekeeping: remove all globally expired / already-used tokens
        await PurgeExpiredTokensAsync();

        // Wipe any prior pending tokens for this specific email
        var existing = await _db.PasswordResetTokens
            .Where(t => t.Email == model.Email)
            .ToListAsync();
        _db.PasswordResetTokens.RemoveRange(existing);

        // Look up admin — but NEVER reveal to the caller whether the email exists
        var admin = await _db.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == model.Email);

        if (admin is not null)
        {
            var code = GenerateCode();

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Email      = model.Email,
                Code       = code,
                ExpiryTime = DateTime.UtcNow.AddMinutes(15),
                IsUsed     = false
            });
            await _db.SaveChangesAsync();

            try { await _email.SendPasswordResetCodeAsync(model.Email, code); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email service threw for {Email}", model.Email);
            }
        }
        else
        {
            await _db.SaveChangesAsync();
        }

        // Same message regardless of whether the email existed (anti-enumeration)
        TempData["ResetEmail"] = model.Email;
        TempData["ForgotInfo"] = "If that email is registered, a 6-digit code has been sent. Check your inbox (and spam folder).";

        return RedirectToAction(nameof(VerifyCode));
    }

    // =========================================================================
    // STEP 2 — Verify Code
    // =========================================================================

    // GET /admin/verify-code
    [HttpGet("verify-code")]
    public IActionResult VerifyCode()
    {
        var email = TempData.Peek("ResetEmail")?.ToString();

        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(ForgotPassword));

        ViewBag.Info = TempData["ForgotInfo"]?.ToString();
        return View(VerifyView, new VerifyCodeViewModel { Email = email });
    }

    // POST /admin/verify-code
    [HttpPost("verify-code")]
    [ValidateAntiForgeryToken]
    [ActionName("VerifyCode")]
    public async Task<IActionResult> VerifyCodePost(VerifyCodeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(VerifyView, model);

        var token = await _db.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.Email  == model.Email &&
                t.Code   == model.Code  &&
                !t.IsUsed &&
                t.ExpiryTime > DateTime.UtcNow);

        if (token is null)
        {
            // Vague message: don't reveal which field was wrong
            ModelState.AddModelError(string.Empty,
                "The code is invalid or has expired. Please request a new one.");
            return View(VerifyView, model);
        }

        // Code verified — pass context to the reset step via TempData
        TempData["ResetEmail"] = model.Email;
        TempData["ResetCode"]  = model.Code;

        return RedirectToAction(nameof(ResetPassword));
    }

    // =========================================================================
    // STEP 3 — Reset Password
    // =========================================================================

    // GET /admin/reset-password
    [HttpGet("reset-password")]
    public IActionResult ResetPassword()
    {
        var email = TempData.Peek("ResetEmail")?.ToString();
        var code  = TempData.Peek("ResetCode")?.ToString();

        // Guard: must arrive here from a completed VerifyCode step
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            return RedirectToAction(nameof(ForgotPassword));

        return View(ResetView, new ResetPasswordViewModel { Email = email, Code = code });
    }

    // POST /admin/reset-password
    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    [ActionName("ResetPassword")]
    public async Task<IActionResult> ResetPasswordPost(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(ResetView, model);

        // Re-validate the token (prevents replayed form submissions after expiry)
        var token = await _db.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.Email  == model.Email &&
                t.Code   == model.Code  &&
                !t.IsUsed &&
                t.ExpiryTime > DateTime.UtcNow);

        if (token is null)
        {
            TempData["ResetError"] = "Your reset session has expired or was already used. Please start again.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var admin = await _db.AdminUsers
            .FirstOrDefaultAsync(a => a.Email == model.Email);

        if (admin is null)
        {
            TempData["ResetError"] = "Account not found.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        // Hash and persist the new password
        admin.PasswordHash = _hasher.HashPassword(admin, model.NewPassword);

        // One-time use: mark the token
        token.IsUsed = true;

        await _db.SaveChangesAsync();

        // Clean up all remaining tokens for this email
        await PurgeTokensByEmailAsync(model.Email);

        _logger.LogInformation("Admin password successfully reset for {Email}", model.Email);

        TempData["LoginSuccess"] = "Password reset successfully. Please sign in with your new password.";
        return RedirectToAction("Page", "Auth");
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static string GenerateCode()
    {
        // Cryptographically random 6-digit code (000000 – 999999)
        var bytes  = RandomNumberGenerator.GetBytes(4);
        var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000u;
        return number.ToString("D6");
    }

    private async Task PurgeExpiredTokensAsync()
    {
        var stale = await _db.PasswordResetTokens
            .Where(t => t.ExpiryTime < DateTime.UtcNow || t.IsUsed)
            .ToListAsync();

        if (stale.Count > 0)
        {
            _db.PasswordResetTokens.RemoveRange(stale);
            await _db.SaveChangesAsync();
        }
    }

    private async Task PurgeTokensByEmailAsync(string email)
    {
        var tokens = await _db.PasswordResetTokens
            .Where(t => t.Email == email)
            .ToListAsync();

        if (tokens.Count > 0)
        {
            _db.PasswordResetTokens.RemoveRange(tokens);
            await _db.SaveChangesAsync();
        }
    }
}
