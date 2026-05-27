using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/experience")]
[Authorize]
public class ExperienceController : Controller
{
    private readonly AppDbContext _db;
    public ExperienceController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Experiences.OrderByDescending(e => e.StartDate).ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Experience { StartDate = DateTime.Today });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Experience model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Experiences.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Experience at \"{model.Company}\" added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var exp = await _db.Experiences.FindAsync(id);
        if (exp is null) return NotFound();
        return View(exp);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Experience model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Experiences.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Experience at \"{model.Company}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var exp = await _db.Experiences.FindAsync(id);
        if (exp is null) return NotFound();
        _db.Experiences.Remove(exp);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Experience at \"{exp.Company}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
