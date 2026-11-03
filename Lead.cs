using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Models;
public class Lead
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Phone] public string? Phone { get; set; }
    [StringLength(100)] public string? Source { get; set; }
    [Required] public string Status { get; set; } = "New";
    [StringLength(450)] public string? AssignedUserId { get; set; }
    [StringLength(20)] public string Priority { get; set; } = "Normal";
    [StringLength(1000)] public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
