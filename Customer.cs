using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Models;
public class Customer
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(25)] public string Phone { get; set; } = "";
    [StringLength(120)] public string? Company { get; set; }
    [StringLength(250)] public string? Address { get; set; }
    [StringLength(30)] public string Status { get; set; } = "Active";
    [StringLength(450)] public string? AssignedUserId { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
