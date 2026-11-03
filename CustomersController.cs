using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;
[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db; private readonly AuditService _audit;
    public CustomersController(ApplicationDbContext db, AuditService audit) { _db=db; _audit=audit; }
    public async Task<IActionResult> Index(string? search) { var q=_db.Customers.AsQueryable(); if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.Name.Contains(search)||x.Email.Contains(search)|| (x.Company??"").Contains(search)); ViewBag.Search=search; return View(await q.OrderByDescending(x=>x.CreatedDate).ToListAsync()); }
    public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(Customer model) { if(await _db.Customers.AnyAsync(x=>x.Email==model.Email)) ModelState.AddModelError("Email","Email already exists."); if(await _db.Customers.AnyAsync(x=>x.Phone==model.Phone)) ModelState.AddModelError("Phone","Phone already exists."); if(!ModelState.IsValid)return View(model); _db.Customers.Add(model); await _db.SaveChangesAsync(); await _audit.RecordAsync("Create","Customer",model.Id.ToString(),null,model); return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Edit(int id) { var x=await _db.Customers.FindAsync(id); return x==null?NotFound():View(x); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, Customer model) { if(id!=model.Id)return BadRequest(); if(await _db.Customers.AnyAsync(x=>x.Id!=id&&(x.Email==model.Email||x.Phone==model.Phone)))ModelState.AddModelError("Email","Email or phone already belongs to another customer."); if(!ModelState.IsValid)return View(model); var old=await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id); if(old==null)return NotFound(); model.CreatedDate=old.CreatedDate; _db.Update(model); await _db.SaveChangesAsync(); await _audit.RecordAsync("Update","Customer",id.ToString(),old,model); return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Delete(int id) { var x=await _db.Customers.FindAsync(id); return x==null?NotFound():View(x); }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id) { var x=await _db.Customers.FindAsync(id); if(x!=null){_db.Remove(x);await _db.SaveChangesAsync();await _audit.RecordAsync("Delete","Customer",id.ToString(),x,null);} return RedirectToAction(nameof(Index)); }
}
