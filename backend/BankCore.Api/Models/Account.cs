namespace BankCore.Api.Models;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string AccountType { get; set; } = "Savings";
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "LKR";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
