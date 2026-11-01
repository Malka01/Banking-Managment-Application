namespace BankCore.Api.Models;

public class AuditLog
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
