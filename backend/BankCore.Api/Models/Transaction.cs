namespace BankCore.Api.Models;

public class Transaction
{
    public long Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
