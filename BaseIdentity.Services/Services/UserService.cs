using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaseIdentity.Data.Identity;
using BaseIdentity.Services.Interfaces;

namespace BaseIdentity.Services.Services;

public class UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : IUserService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<AppRole> _roleManager = roleManager;

    public async Task<List<AppUser>> GetAllAsync() =>
        await _userManager.Users.Include(u => u.Roles).AsNoTracking().ToListAsync();

    public async Task<AppUser?> GetByIdAsync(Guid id) =>
        await _userManager.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> CreateAsync(string email, string password, Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null) return false;

        var user = new AppUser { UserName = email, Email = email };
        var create = await _userManager.CreateAsync(user, password);
        if (!create.Succeeded) return false;

        var addRole = await _userManager.AddToRoleAsync(user, role.Name!);
        return addRole.Succeeded;
    }

    public async Task<bool> UpdateRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null) return false;

        var current = await _userManager.GetRolesAsync(user);
        if (current.Any()) await _userManager.RemoveFromRolesAsync(user, current);
        var add = await _userManager.AddToRoleAsync(user, role.Name!);
        return add.Succeeded;
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var del = await _userManager.DeleteAsync(user);
        return del.Succeeded;
    }
}
