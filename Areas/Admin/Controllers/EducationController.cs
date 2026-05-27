using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/education")]
[Authorize]
public class EducationController : Controller
{
    private readonly AppDbContext _db;
    public EducationController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Educations.OrderByDescending(e => e.StartDate).ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Education { StartDate = DateTime.Today });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Education model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Educations.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Education at \"{model.School}\" added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var edu = await _db.Educations.FindAsync(id);
        if (edu is null) return NotFound();
        return View(edu);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Education model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Educations.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Education at \"{model.School}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var edu = await _db.Educations.FindAsync(id);
        if (edu is null) return NotFound();
        _db.Educations.Remove(edu);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Education at \"{edu.School}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
