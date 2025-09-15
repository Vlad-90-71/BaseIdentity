namespace BaseIdentity.Data.Entity;

public class AdminActionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string AdminEmail { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty; // CreateUser, DeleteUser, CreateRole, DeleteRole, UpdateUserRole
    public string Target { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
