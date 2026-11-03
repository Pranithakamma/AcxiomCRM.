using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers.Api;

[ApiController, Route("api/opportunities"), Authorize]
public class OpportunitiesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public OpportunitiesApiController(ApplicationDbContext db) => _db = db;
    [HttpGet] public async Task<ActionResult<IEnumerable<OpportunityDto>>> Get() => Ok(await _db.Opportunities.OrderBy(x => x.ExpectedCloseDate).Select(x => new OpportunityDto(x.Id, x.Name, x.Stage, x.Amount, x.Probability, x.Amount*x.Probability/100m, x.ExpectedCloseDate)).ToListAsync());
    [HttpGet("{id:int}")] public async Task<ActionResult<OpportunityDto>> Get(int id) { var x = await _db.Opportunities.FindAsync(id); return x == null ? NotFound() : Ok(new OpportunityDto(x.Id,x.Name,x.Stage,x.Amount,x.Probability,x.Amount*x.Probability/100m,x.ExpectedCloseDate)); }
    [HttpPost] public async Task<ActionResult<OpportunityDto>> Post(Opportunity dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (dto.Amount <= 0) return BadRequest(new { message = "Amount must be greater than zero." });
        if (dto.Probability is < 0 or > 100) return BadRequest(new { message = "Probability must be between 0 and 100." });
        if (dto.Stage is not ("Won" or "Lost") && dto.ExpectedCloseDate.Date < DateTime.Today) return BadRequest(new { message = "Expected close date cannot be in the past for an active opportunity." });
        dto.Id = 0; dto.CreatedDate = DateTime.UtcNow; _db.Opportunities.Add(dto); await _db.SaveChangesAsync();
        var response = new OpportunityDto(dto.Id,dto.Name,dto.Stage,dto.Amount,dto.Probability,dto.Amount*dto.Probability/100m,dto.ExpectedCloseDate);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, response);
    }
}
public record OpportunityDto(int Id, string Name, string Stage, decimal Amount, int Probability, decimal WeightedAmount, DateTime ExpectedCloseDate);
