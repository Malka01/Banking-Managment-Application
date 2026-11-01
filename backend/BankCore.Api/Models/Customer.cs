namespace BankCore.Api.Models;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Nic { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Account> Accounts { get; set; } = [];
}
