using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BaseIdentity.Data.Identity;
using BaseIdentity.Services.Interfaces;
using BaseIdentity.Web.Models;

namespace BaseIdentity.Web.Controllers;

public class AccountController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    RoleManager<AppRole> roleManager,
    IEmailSender emailSender) : Controller
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly RoleManager<AppRole> _roleManager = roleManager;
    private readonly IEmailSender _emailSender = emailSender;

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Roles = _roleManager.Roles.ToList();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(model);
        }

        var role = await _roleManager.FindByIdAsync(model.RoleId.ToString());
        if (role == null)
        {
            ModelState.AddModelError("", "Role not found");
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(model);
        }

        var user = new AppUser { UserName = model.Email, Email = model.Email };
        var create = await _userManager.CreateAsync(user, model.Password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors) ModelState.AddModelError("", e.Description);
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, role.Name!);

        // Email confirmation
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);
        await _emailSender.SendEmailAsync(user.Email!, "Confirm your email",
            $"<p>Please confirm your account by <a href='{link}'>clicking here</a>.</p>");

        TempData["Message"] = "Registration successful. Check your email to confirm your account.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            TempData["Message"] = "Email confirmed. You can now log in.";
            return RedirectToAction("Login");
        }
        TempData["Error"] = "Email confirmation failed.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
        if (result.Succeeded) return RedirectToAction("Index", "Home");
        if (result.IsNotAllowed) ModelState.AddModelError("", "Email not confirmed.");
        else ModelState.AddModelError("", "Invalid login attempt");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
