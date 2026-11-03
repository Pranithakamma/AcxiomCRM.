using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly AuditService _audit;
    public UsersController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, AuditService audit)
    { _users = users; _roles = roles; _audit = audit; }

    public async Task<IActionResult> Index()
    {
        var users = await _users.Users.OrderBy(x => x.Email).ToListAsync();
        var rows = new List<UserRow>();
        foreach (var user in users)
            rows.Add(new UserRow(user.Id, user.FullName ?? "", user.Email ?? "", (await _users.GetRolesAsync(user)).FirstOrDefault() ?? "No role", user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow));
        return View(rows);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roles.Roles.Select(x => x.Name!).OrderBy(x => x).ToListAsync();
        return View(new CreateUserModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserModel model)
    {
        ViewBag.Roles = await _roles.Roles.Select(x => x.Name!).OrderBy(x => x).ToListAsync();
        if (!ModelState.IsValid) return View(model);
        if (!new[] { "Admin", "Manager", "SalesExecutive" }.Contains(model.Role))
        { ModelState.AddModelError(nameof(model.Role), "Select a valid role."); return View(model); }
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, EmailConfirmed = true };
        var result = await _users.CreateAsync(user, model.TemporaryPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        var roleResult = await _users.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user);
            foreach (var error in roleResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        await _audit.RecordAsync("CreateUserAndAssignRole", "User", user.Id, null, new { user.Email, model.Role });
        TempData["Message"] = "User created. Share the temporary password securely and ask the user to change it.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string id, string role)
    {
        if (!new[] { "Admin", "Manager", "SalesExecutive" }.Contains(role)) return BadRequest("Invalid role.");
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();
        if (user.Id == _users.GetUserId(User) && role != "Admin") { TempData["Message"] = "You cannot remove your own Admin role here."; return RedirectToAction(nameof(Index)); }
        var oldRoles = await _users.GetRolesAsync(user);
        if (oldRoles.Contains("Admin") && role != "Admin" && (await _users.GetUsersInRoleAsync("Admin")).Count <= 1)
        { TempData["Message"] = "The last Admin cannot be demoted."; return RedirectToAction(nameof(Index)); }
        var remove = await _users.RemoveFromRolesAsync(user, oldRoles);
        if (!remove.Succeeded) { TempData["Message"] = string.Join("; ", remove.Errors.Select(x => x.Description)); return RedirectToAction(nameof(Index)); }
        var add = await _users.AddToRoleAsync(user, role);
        if (!add.Succeeded) { await _users.AddToRolesAsync(user, oldRoles); TempData["Message"] = string.Join("; ", add.Errors.Select(x => x.Description)); return RedirectToAction(nameof(Index)); }
        await _audit.RecordAsync("ChangeUserRole", "User", user.Id, new { Roles = oldRoles }, new { Role = role });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();
        if (user.Id == _users.GetUserId(User)) { TempData["Message"] = "You cannot lock your own account from this screen."; return RedirectToAction(nameof(Index)); }
        var locked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
        user.LockoutEnabled = true;
        user.LockoutEnd = locked ? null : DateTimeOffset.UtcNow.AddYears(100);
        await _users.UpdateAsync(user);
        await _audit.RecordAsync(locked ? "ActivateUser" : "DeactivateUser", "User", user.Id, null, new { user.Email });
        return RedirectToAction(nameof(Index));
    }
}

public record UserRow(string Id, string FullName, string Email, string Role, bool Locked);
public class CreateUserModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(100)] public string FullName { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress] public string Email { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8), System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)] public string TemporaryPassword { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required] public string Role { get; set; } = "SalesExecutive";
}
