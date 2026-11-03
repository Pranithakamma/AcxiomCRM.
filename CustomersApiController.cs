using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers.Api;

[ApiController, Route("api/customers"), Authorize]
public class CustomersApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public CustomersApiController(ApplicationDbContext db) => _db = db;
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> Get() { var customers = await _db.Customers.OrderBy(x => x.Name).ToListAsync(); return Ok(customers.Select(ToDto)); }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> Get(int id) { var c = await _db.Customers.FindAsync(id); return c == null ? NotFound() : Ok(ToDto(c)); }
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Post(CustomerCreateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (await _db.Customers.AnyAsync(x => x.Email == dto.Email || x.Phone == dto.Phone)) return Conflict(new { message = "Email or phone already exists." });
        var c = new Customer { Name = dto.Name, Email = dto.Email, Phone = dto.Phone, Company = dto.Company, Address = dto.Address, Status = dto.Status ?? "Active", Notes = dto.Notes, CreatedDate = DateTime.UtcNow };
        _db.Customers.Add(c); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, ToDto(c));
    }
    private static CustomerDto ToDto(Customer c) => new(c.Id, c.Name, c.Email, c.Phone, c.Company, c.Address, c.Status, c.CreatedDate);
}
public record CustomerDto(int Id, string Name, string Email, string Phone, string? Company, string? Address, string Status, DateTime CreatedDate);
public class CustomerCreateDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(100)] public string Name { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress, System.ComponentModel.DataAnnotations.StringLength(150)] public string Email { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.Phone] public string Phone { get; set; } = "";
    [System.ComponentModel.DataAnnotations.StringLength(120)] public string? Company { get; set; }
    [System.ComponentModel.DataAnnotations.StringLength(250)] public string? Address { get; set; }
    [System.ComponentModel.DataAnnotations.StringLength(30)] public string? Status { get; set; }
    [System.ComponentModel.DataAnnotations.StringLength(1000)] public string? Notes { get; set; }
}
