using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/skills")]
[Authorize]
public class SkillsController : Controller
{
    private readonly AppDbContext _db;
    public SkillsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var skills = await _db.Skills.OrderByDescending(s => s.Level).ToListAsync();
        return View(skills);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Skill());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Skill model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Skills.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Skill \"{model.Name}\" added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var skill = await _db.Skills.FindAsync(id);
        if (skill is null) return NotFound();
        return View(skill);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Skill model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Skills.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Skill \"{model.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var skill = await _db.Skills.FindAsync(id);
        if (skill is null) return NotFound();
        _db.Skills.Remove(skill);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Skill \"{skill.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
