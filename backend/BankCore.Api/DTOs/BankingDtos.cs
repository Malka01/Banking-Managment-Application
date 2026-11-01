using System.ComponentModel.DataAnnotations;

namespace BankCore.Api.DTOs;

public record CustomerDto(int Id, string FullName, string Nic, string Email, string PhoneNumber, string Address, bool IsActive, DateTimeOffset CreatedAt);
public record AccountDto(int Id, string AccountNumber, int CustomerId, string CustomerName, string AccountType, decimal Balance, string Currency, bool IsActive, DateTimeOffset CreatedAt);
public record TransactionDto(long Id, string ReferenceNumber, int AccountId, string AccountNumber, string Type, decimal Amount, decimal PreviousBalance, decimal NewBalance, string? Description, DateTimeOffset CreatedAt);
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public class CustomerRequest
{
    [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Nic { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Address { get; set; } = string.Empty;
}

public class CustomerUpdateRequest
{
    [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, StringLength(300)] public string Address { get; set; } = string.Empty;
}

public class AccountRequest
{
    [Required, StringLength(20)] public string AccountNumber { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int CustomerId { get; set; }
    [Required] public string AccountType { get; set; } = "Savings";
    [Range(0, double.MaxValue)] public decimal OpeningBalance { get; set; }
    [StringLength(3)] public string Currency { get; set; } = "LKR";
}

public class MoneyRequest
{
    [Required] public string AccountNumber { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Amount { get; set; }
    [StringLength(250)] public string? Description { get; set; }
}

public class TransferRequest
{
    [Required] public string FromAccountNumber { get; set; } = string.Empty;
    [Required] public string ToAccountNumber { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Amount { get; set; }
    [StringLength(250)] public string? Description { get; set; }
}
