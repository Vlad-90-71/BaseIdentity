using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BaseIdentity.Services.Models;
using BaseIdentity.Services.Interfaces;
using BaseIdentity.Web.Models;

namespace BaseIdentity.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController(
    IUserService userService,
    IRoleService roleService,
    IAuditService auditService) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly IRoleService _roleService = roleService;
    private readonly IAuditService _auditService = auditService;

    // Users

    public async Task<IActionResult> Users()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        ViewBag.Roles = await _roleService.GetAllAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleService.GetAllAsync();
            return View(model);
        }

        var success = await _userService.CreateAsync(model.Email, model.Password, model.RoleId);
        if (!success)
        {
            ModelState.AddModelError("", "Error creating user");
            ViewBag.Roles = await _roleService.GetAllAsync();
            return View(model);
        }

        await _auditService.LogAsync(User.Identity!.Name!, "CreateUser", model.Email);
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var currentRoleName = user.Roles.FirstOrDefault()?.Name ?? string.Empty;
        var currentRoleId = await _roleService.GetRoleIdByNameAsync(currentRoleName);

        var vm = new EditUserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            CurrentRoleId = currentRoleId,
            Roles = await _roleService.GetAllAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(EditUserRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = await _roleService.GetAllAsync();
            return View(model);
        }

        var success = await _userService.UpdateRoleAsync(model.UserId, model.NewRoleId);
        if (!success)
        {
            ModelState.AddModelError("", "Error updating role");
            model.Roles = await _roleService.GetAllAsync();
            return View(model);
        }

        await _auditService.LogAsync(User.Identity!.Name!, "UpdateUserRole", model.Email, $"NewRoleId={model.NewRoleId}");
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id, string email)
    {
        var success = await _userService.DeleteAsync(id);
        if (!success) TempData["Error"] = "Failed to delete user";
        else await _auditService.LogAsync(User.Identity!.Name!, "DeleteUser", email);
        return RedirectToAction(nameof(Users));
    }

    // Roles
    public async Task<IActionResult> Roles()
    {
        var roles = await _roleService.GetAllAsync();
        return View(roles);
    }

    [HttpGet]
    public IActionResult CreateRole() => View(new CreateRoleViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _roleService.CreateAsync(model.Name);
        if (!success)
        {
            ModelState.AddModelError("", "Role already exists or invalid");
            return View(model);
        }

        await _auditService.LogAsync(User.Identity!.Name!, "CreateRole", model.Name);
        return RedirectToAction(nameof(Roles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(Guid id, string name)
    {
        var success = await _roleService.DeleteAsync(id);
        if (!success) TempData["Error"] = "Cannot delete role (maybe it has users)";
        else await _auditService.LogAsync(User.Identity!.Name!, "DeleteRole", name);
        return RedirectToAction(nameof(Roles));
    }

    // Audit

    [HttpGet]
    public async Task<IActionResult> AuditLog([FromQuery] AuditLogFilterViewModel filter)
    {
        var query = new AuditQuery
        {
            AdminEmail = filter.AdminEmail,
            ActionType = filter.ActionType,
            FromUtc = filter.FromUtc,
            ToUtc = filter.ToUtc,
            SortBy = filter.SortBy ?? "Timestamp",
            Desc = filter.Desc,
            Page = Math.Max(1, filter.Page),
            PageSize = Math.Clamp(filter.PageSize, 1, 200)
        };

        var result = await _auditService.GetPagedAsync(query);
        filter.AvailableActionTypes =
            ["CreateUser", "DeleteUser", "CreateRole", "DeleteRole", "UpdateUserRole"];
        return View((result, filter));
    }
}
