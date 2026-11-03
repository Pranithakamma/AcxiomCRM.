using AcxiomCRM.Data; using AcxiomCRM.Models; using AcxiomCRM.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;
[Authorize] public class FollowUpsController : Controller {
 private readonly ApplicationDbContext _db; private readonly AuditService _audit; public FollowUpsController(ApplicationDbContext db,AuditService audit){_db=db;_audit=audit;}
 public async Task<IActionResult> Index()=>View(await _db.FollowUps.OrderBy(x=>x.FollowUpDate).ToListAsync());
 public IActionResult Create()=>View(new FollowUp());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(FollowUp m){if(!m.Completed&&m.FollowUpDate.Date<DateTime.Today)ModelState.AddModelError("FollowUpDate","A planned follow-up cannot be before today.");if(!ModelState.IsValid)return View(m);_db.Add(m);await _db.SaveChangesAsync();await _audit.RecordAsync("Create","FollowUp",m.Id.ToString(),null,m);return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id){var x=await _db.FollowUps.FindAsync(id);if(x!=null){_db.Remove(x);await _db.SaveChangesAsync();await _audit.RecordAsync("Delete","FollowUp",id.ToString(),x,null);}return RedirectToAction(nameof(Index));}
}
