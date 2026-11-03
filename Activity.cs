using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Models;
public class Activity
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Subject { get; set; } = "";
    [Required] public string ActivityType { get; set; } = "Call";
    public DateTime ActivityDate { get; set; } = DateTime.Today;
    [StringLength(1000)] public string? Notes { get; set; }
}
