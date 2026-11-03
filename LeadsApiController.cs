using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers.Api;

[ApiController, Route("api/leads"), Authorize]
public class LeadsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public LeadsApiController(ApplicationDbContext db) => _db = db;
    [HttpGet] public async Task<ActionResult<IEnumerable<Lead>>> Get() => Ok(await _db.Leads.OrderByDescending(x => x.CreatedDate).ToListAsync());
    [HttpGet("{id:int}")] public async Task<ActionResult<Lead>> Get(int id) { var x = await _db.Leads.FindAsync(id); return x == null ? NotFound() : Ok(x); }
    [HttpPost] public async Task<ActionResult<Lead>> Post(Lead dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!new[] { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" }.Contains(dto.Status)) return BadRequest(new { message = "Invalid lead status." });
        dto.Id = 0; dto.CreatedDate = DateTime.UtcNow; _db.Leads.Add(dto); await _db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }
}
