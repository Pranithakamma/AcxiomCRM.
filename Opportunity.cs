using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Models;
public class Opportunity
{
    public int Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Range(0.01, 999999999)] public decimal Amount { get; set; }
    [Range(0, 100)] public int Probability { get; set; }
    [Required] public string Stage { get; set; } = "Prospecting";
    [DataType(DataType.Date)] public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(30);
    public int? CustomerId { get; set; }
    [StringLength(450)] public string? OwnerUserId { get; set; }
    [StringLength(100)] public string? Source { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
