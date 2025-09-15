using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaseIdentity.Data.Identity;
using BaseIdentity.Services.Interfaces;

namespace BaseIdentity.Services.Services;

public class RoleService(RoleManager<AppRole> roleManager) : IRoleService
{
    private readonly RoleManager<AppRole> _roleManager = roleManager;

    public async Task<List<AppRole>> GetAllAsync() =>
        await _roleManager.Roles.Include(r => r.Users).AsNoTracking().ToListAsync();

    public async Task<Guid?> GetRoleIdByNameAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return null;
        var role = await _roleManager.FindByNameAsync(roleName);
        return role?.Id;
    }

    public async Task<bool> CreateAsync(string name)
    {
        if (await _roleManager.RoleExistsAsync(name)) return false;
        var role = new AppRole { Name = name, NormalizedName = name.ToUpperInvariant() };
        var create = await _roleManager.CreateAsync(role);
        return create.Succeeded;
    }

    public async Task<bool> DeleteAsync(Guid roleId)
    {
        var role = await _roleManager.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null || role.Users.Count != 0) return false;
        var del = await _roleManager.DeleteAsync(role);
        return del.Succeeded;
    }
}
