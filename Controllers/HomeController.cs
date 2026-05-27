using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;
using suneer_web.Models;
using suneer_web.Models.ViewModels;

namespace suneer_web.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    // GET /
    [HttpGet("")]
    [HttpGet("home")]
    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            Profile          = await _db.Profiles.FirstOrDefaultAsync(),
            FeaturedSkills   = await _db.Skills
                                   .OrderByDescending(s => s.Level)
                                   .Take(6)
                                   .ToListAsync(),
            FeaturedProjects = await _db.Projects
                                   .Take(3)
                                   .ToListAsync()
        };
        return View(vm);
    }

    // GET /about
    [HttpGet("about")]
    public async Task<IActionResult> About()
    {
        var vm = new AboutViewModel
        {
            Profile     = await _db.Profiles.FirstOrDefaultAsync(),
            Experiences = await _db.Experiences
                              .OrderByDescending(e => e.StartDate)
                              .ToListAsync(),
            Educations  = await _db.Educations
                              .OrderByDescending(e => e.StartDate)
                              .ToListAsync()
        };
        return View(vm);
    }

    // GET /resume
    [HttpGet("resume")]
    public async Task<IActionResult> Resume()
    {
        var vm = new ResumeViewModel
        {
            Skills      = await _db.Skills
                              .OrderByDescending(s => s.Level)
                              .ToListAsync(),
            Experiences = await _db.Experiences
                              .OrderByDescending(e => e.StartDate)
                              .ToListAsync(),
            Educations  = await _db.Educations
                              .OrderByDescending(e => e.StartDate)
                              .ToListAsync()
        };
        return View(vm);
    }

    // GET /projects
    [HttpGet("projects")]
    public async Task<IActionResult> Projects()
    {
        var projects = await _db.Projects.ToListAsync();

        var techTags = projects
            .SelectMany(p => p.TechStack.Split(',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var vm = new ProjectsViewModel
        {
            Projects = projects,
            TechTags = techTags
        };
        return View(vm);
    }

    // GET /contact
    [HttpGet("contact")]
    public IActionResult Contact()
    {
        return View(new ContactViewModel());
    }

    // POST /contact
    [HttpPost("contact")]
    [ValidateAntiForgeryToken]
    [ActionName("Contact")]
    public async Task<IActionResult> ContactPost(ContactViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.ContactMessages.Add(new ContactMessage
        {
            Name      = model.Name,
            Email     = model.Email,
            Message   = model.Message,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Thanks {model.Name}! Your message has been received. I'll get back to you soon.";
        return RedirectToAction(nameof(Contact));
    }

}
