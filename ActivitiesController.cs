using AcxiomCRM.Data; using AcxiomCRM.Models; using AcxiomCRM.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;
[Authorize] public class ActivitiesController : Controller {
 private readonly ApplicationDbContext _db; private readonly AuditService _audit; public ActivitiesController(ApplicationDbContext db,AuditService audit){_db=db;_audit=audit;}
 public async Task<IActionResult> Index()=>View(await _db.Activities.OrderByDescending(x=>x.ActivityDate).ToListAsync());
 public IActionResult Create()=>View(new Activity());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(Activity m){if(!ModelState.IsValid)return View(m);_db.Add(m);await _db.SaveChangesAsync();await _audit.RecordAsync("Create","Activity",m.Id.ToString(),null,m);return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id){var x=await _db.Activities.FindAsync(id);if(x!=null){_db.Remove(x);await _db.SaveChangesAsync();await _audit.RecordAsync("Delete","Activity",id.ToString(),x,null);}return RedirectToAction(nameof(Index));}
}
