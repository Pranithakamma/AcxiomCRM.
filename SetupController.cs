using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

// First-run bootstrap only: the first signed-in account can claim Admin if no Admin exists.
// Remove this controller after the initial administrator has been assigned in a deployed environment.
[Authorize]
public class SetupController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;
    public SetupController(UserManager<ApplicationUser> users, AuditService audit) { _users = users; _audit = audit; }

    [HttpGet]
    public async Task<IActionResult> ClaimAdmin()
    {
        if ((await _users.GetUsersInRoleAsync("Admin")).Count > 0) return Forbid();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimAdmin(string confirm)
    {
        if (confirm != "CLAIM ADMIN") return BadRequest("Type CLAIM ADMIN to confirm.");
        if ((await _users.GetUsersInRoleAsync("Admin")).Count > 0) return Forbid();
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        var result = await _users.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded) return BadRequest(string.Join("; ", result.Errors.Select(x => x.Description)));
        await _audit.RecordAsync("BootstrapAdminClaim", "User", user.Id, null, new { user.Email });
        TempData["Message"] = "Admin role assigned. Remove SetupController before production deployment.";
        return RedirectToAction("Index", "Home");
    }
}
