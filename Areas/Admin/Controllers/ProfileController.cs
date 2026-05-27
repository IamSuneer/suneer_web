using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/profile")]
[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Edit()
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync() ?? new Profile();
        return View(profile);
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Profile model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.Id == 0)
            _db.Profiles.Add(model);
        else
            _db.Profiles.Update(model);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Edit));
    }
}
