using BaseIdentity.Data.Identity;

namespace BaseIdentity.Services.Interfaces;

public interface IRoleService
{
    Task<List<AppRole>> GetAllAsync();
    Task<Guid?> GetRoleIdByNameAsync(string roleName);
    Task<bool> CreateAsync(string name);
    Task<bool> DeleteAsync(Guid roleId);
}
