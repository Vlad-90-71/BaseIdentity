namespace BaseIdentity.Web.Models;

public class AuditLogFilterViewModel
{
    public string? AdminEmail { get; set; }
    public string? ActionType { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }

    public string? SortBy { get; set; }
    public bool Desc { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public IEnumerable<string> AvailableActionTypes { get; set; } = [];
}
