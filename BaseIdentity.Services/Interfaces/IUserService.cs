using BaseIdentity.Data.Identity;

namespace BaseIdentity.Services.Interfaces;

public interface IUserService
{
    Task<List<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(string email, string password, Guid roleId);
    Task<bool> UpdateRoleAsync(Guid userId, Guid roleId);
    Task<bool> DeleteAsync(Guid userId);
}
