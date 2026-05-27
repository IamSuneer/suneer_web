using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        ViewBag.ProjectCount    = await _db.Projects.CountAsync();
        ViewBag.SkillCount      = await _db.Skills.CountAsync();
        ViewBag.ExperienceCount = await _db.Experiences.CountAsync();
        ViewBag.MessageCount    = await _db.ContactMessages.CountAsync();

        ViewBag.RecentMessages = await _db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View();
    }
}
