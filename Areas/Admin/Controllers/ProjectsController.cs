using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/projects")]
[Authorize]
public class ProjectsController : Controller
{
    private readonly AppDbContext _db;
    public ProjectsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Projects.OrderByDescending(p => p.Id).ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Project());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Projects.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Project \"{model.Title}\" added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project is null) return NotFound();
        return View(project);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Projects.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Project \"{model.Title}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project is null) return NotFound();
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Project \"{project.Title}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
