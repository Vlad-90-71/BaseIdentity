using Microsoft.EntityFrameworkCore;
using BaseIdentity.Data;
using BaseIdentity.Data.Entity;
using BaseIdentity.Services.Models;
using BaseIdentity.Services.Interfaces;

namespace BaseIdentity.Services.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    private readonly AppDbContext _db = db;

    public async Task LogAsync(string adminEmail, string actionType, string target, string details = "")
    {
        _db.AdminActionLogs.Add(new AdminActionLog
        {
            AdminEmail = adminEmail,
            ActionType = actionType,
            Target = target,
            Details = details
        });
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<AdminActionLog>> GetPagedAsync(AuditQuery query)
    {
        var q = _db.AdminActionLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.AdminEmail))
            q = q.Where(x => x.AdminEmail.Contains(query.AdminEmail));
        if (!string.IsNullOrWhiteSpace(query.ActionType))
            q = q.Where(x => x.ActionType == query.ActionType);
        if (query.FromUtc.HasValue)
            q = q.Where(x => x.Timestamp >= query.FromUtc.Value);
        if (query.ToUtc.HasValue)
            q = q.Where(x => x.Timestamp <= query.ToUtc.Value);

        q = (query.SortBy, query.Desc) switch
        {
            ("AdminEmail", true) => q.OrderByDescending(x => x.AdminEmail),
            ("AdminEmail", false) => q.OrderBy(x => x.AdminEmail),
            ("ActionType", true) => q.OrderByDescending(x => x.ActionType),
            ("ActionType", false) => q.OrderBy(x => x.ActionType),
            ("Target", true) => q.OrderByDescending(x => x.Target),
            ("Target", false) => q.OrderBy(x => x.Target),
            (_, true) => q.OrderByDescending(x => x.Timestamp),
            _ => q.OrderBy(x => x.Timestamp),
        };

        var total = await q.CountAsync();
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 200);

        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(x => new AdminActionLog
            {
                Timestamp = x.Timestamp,
                AdminEmail = x.AdminEmail,
                ActionType = x.ActionType,
                Target = x.Target,
                Details = x.Details
            })
            .ToListAsync();

        return new PagedResult<AdminActionLog>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = size
        };
    }
}
