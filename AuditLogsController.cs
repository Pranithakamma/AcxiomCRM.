using AcxiomCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class AuditLogsController : Controller
{
    private readonly ApplicationDbContext _db;
    public AuditLogsController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index(string? userId, string? module, string? action, DateTime? from, DateTime? to)
    {
        var q = _db.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(userId)) q = q.Where(x => x.UserId != null && x.UserId.Contains(userId));
        if (!string.IsNullOrWhiteSpace(module)) q = q.Where(x => x.EntityName.Contains(module));
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action.Contains(action));
        if (from.HasValue) q = q.Where(x => x.CreatedDate >= from.Value.Date);
        if (to.HasValue) q = q.Where(x => x.CreatedDate < to.Value.Date.AddDays(1));
        ViewBag.UserId = userId; ViewBag.Module = module; ViewBag.ActionFilter = action; ViewBag.From = from?.ToString("yyyy-MM-dd"); ViewBag.To = to?.ToString("yyyy-MM-dd");
        return View(await q.OrderByDescending(x => x.CreatedDate).Take(500).ToListAsync());
    }
}
