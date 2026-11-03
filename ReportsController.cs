using AcxiomCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Customers = await _db.Customers.OrderByDescending(x => x.CreatedDate).Take(100).ToListAsync();
        ViewBag.Leads = await _db.Leads.OrderByDescending(x => x.CreatedDate).Take(100).ToListAsync();
        ViewBag.Opportunities = await _db.Opportunities.OrderBy(x => x.Stage).ThenBy(x => x.ExpectedCloseDate).Take(100).ToListAsync();
        ViewBag.FollowUps = await _db.FollowUps.OrderBy(x => x.FollowUpDate).Take(100).ToListAsync();
        ViewBag.AuditLogs = await _db.AuditLogs.OrderByDescending(x => x.CreatedDate).Take(100).ToListAsync();
        ViewBag.CustomerCount = await _db.Customers.CountAsync();
        ViewBag.LeadCount = await _db.Leads.CountAsync();
        ViewBag.FollowUpOverdue = await _db.FollowUps.CountAsync(x => !x.Completed && x.FollowUpDate < DateTime.Today);
        ViewBag.WeightedPipeline = await _db.Opportunities.Where(x => x.Stage != "Won" && x.Stage != "Lost").SumAsync(x => (decimal?)(x.Amount * x.Probability / 100m)) ?? 0m;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Export(string report = "customers")
    {
        var sb = new StringBuilder();
        switch (report.ToLowerInvariant())
        {
            case "customers":
                sb.AppendLine("Id,Name,Email,Phone,Company,Address,CreatedDate");
                foreach (var x in await _db.Customers.OrderBy(x => x.Name).ToListAsync()) sb.AppendLine(string.Join(',', new[] { x.Id.ToString(), x.Name, x.Email, x.Phone, x.Company ?? "", x.Address ?? "", x.CreatedDate.ToString("yyyy-MM-dd") }.Select(Csv)));
                break;
            case "leads":
                sb.AppendLine("Id,Name,Email,Phone,Source,Status,CreatedDate");
                foreach (var x in await _db.Leads.OrderBy(x => x.Status).ToListAsync()) sb.AppendLine(string.Join(',', new[] { x.Id.ToString(), x.Name, x.Email, x.Phone ?? "", x.Source ?? "", x.Status, x.CreatedDate.ToString("yyyy-MM-dd") }.Select(Csv)));
                break;
            case "opportunities":
                sb.AppendLine("Id,Name,Stage,Amount,Probability,WeightedAmount,ExpectedCloseDate");
                foreach (var x in await _db.Opportunities.OrderBy(x => x.Stage).ToListAsync()) sb.AppendLine(string.Join(',', new[] { x.Id.ToString(), x.Name, x.Stage, x.Amount.ToString("0.00"), x.Probability.ToString(), (x.Amount*x.Probability/100m).ToString("0.00"), x.ExpectedCloseDate.ToString("yyyy-MM-dd") }.Select(Csv)));
                break;
            case "followups":
                sb.AppendLine("Id,Subject,FollowUpDate,Status,Notes");
                foreach (var x in await _db.FollowUps.OrderBy(x => x.FollowUpDate).ToListAsync()) sb.AppendLine(string.Join(',', new[] { x.Id.ToString(), x.Subject, x.FollowUpDate.ToString("yyyy-MM-dd"), x.Completed ? "Completed" : x.FollowUpDate.Date < DateTime.Today ? "Overdue" : "Planned", x.Notes ?? "" }.Select(Csv)));
                break;
            case "audit":
                sb.AppendLine("Id,UserId,Action,Module,RecordId,CreatedDate,IPAddress");
                foreach (var x in await _db.AuditLogs.OrderByDescending(x => x.CreatedDate).ToListAsync()) sb.AppendLine(string.Join(',', new[] { x.Id.ToString(), x.UserId ?? "", x.Action, x.EntityName, x.RecordId, x.CreatedDate.ToString("O"), x.IPAddress ?? "" }.Select(Csv)));
                break;
            default: return BadRequest("Supported reports: customers, leads, opportunities, followups, audit.");
        }
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(), "text/csv; charset=utf-8", $"acxiomcrm-{report.ToLowerInvariant()}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
    private static string Csv(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}
