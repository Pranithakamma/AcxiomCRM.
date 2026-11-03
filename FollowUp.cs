using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Models;
public class FollowUp
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Subject { get; set; } = "";
    [Required, DataType(DataType.Date)] public DateTime FollowUpDate { get; set; } = DateTime.Today;
    [StringLength(1000)] public string? Notes { get; set; }
    [StringLength(30)] public string Type { get; set; } = "Call";
    [StringLength(450)] public string? AssignedUserId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }
    public bool Completed { get; set; }
}
