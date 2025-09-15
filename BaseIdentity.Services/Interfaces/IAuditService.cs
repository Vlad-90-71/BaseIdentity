using BaseIdentity.Data.Entity;
using BaseIdentity.Services.Models;

namespace BaseIdentity.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string adminEmail, string actionType, string target, string details = "");
    Task<PagedResult<AdminActionLog>> GetPagedAsync(AuditQuery query);
}
