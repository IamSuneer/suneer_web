using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using suneer_web.Data;

namespace suneer_web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/messages")]
[Authorize]
public class MessagesController : Controller
{
    private readonly AppDbContext _db;
    public MessagesController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var messages = await _db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
        return View(messages);
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var msg = await _db.ContactMessages.FindAsync(id);
        if (msg is null) return NotFound();
        _db.ContactMessages.Remove(msg);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Message deleted.";
        return RedirectToAction(nameof(Index));
    }
}
