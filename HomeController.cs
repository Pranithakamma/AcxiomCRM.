using AcxiomCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index()
    {
        ViewBag.Customers = await _db.Customers.CountAsync();
        ViewBag.Leads = await _db.Leads.CountAsync();
        ViewBag.OpenLeads = await _db.Leads.CountAsync(x => x.Status != "Converted" && x.Status != "Lost");
        ViewBag.Opportunities = await _db.Opportunities.CountAsync();
        ViewBag.Won = await _db.Opportunities.CountAsync(x => x.Stage == "Won");
        ViewBag.Lost = await _db.Opportunities.CountAsync(x => x.Stage == "Lost");
        ViewBag.Pipeline = await _db.Opportunities.Where(x => x.Stage != "Won" && x.Stage != "Lost").SumAsync(x => (decimal?)x.Amount) ?? 0;
        ViewBag.LeadLabels = new[] { "New", "Contacted", "Qualified", "Converted", "Lost" };
        ViewBag.LeadValues = await Task.WhenAll(new[] { "New", "Contacted", "Qualified", "Converted", "Lost" }
            .Select(s => _db.Leads.CountAsync(x => x.Status == s)));
        return View();
    }
    public IActionResult Privacy() => View();
    public IActionResult Error() => View();
}
