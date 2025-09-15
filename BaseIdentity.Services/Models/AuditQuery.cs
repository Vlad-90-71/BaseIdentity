using BaseIdentity.Data.Entity;

namespace BaseIdentity.Services.Models;

public class AuditQuery
{
    public string? AdminEmail { get; set; }
    public string? ActionType { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? SortBy { get; set; } = "Timestamp";
    public bool Desc { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PagedResult<AdminActionLog>.DefaultPageSize;
}
