using BankCore.Api.DTOs;

namespace BankCore.Api.Services;

public interface IBankingService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(string? search, int page, int pageSize, CancellationToken token);
    Task<CustomerDto> CreateCustomerAsync(CustomerRequest request, CancellationToken token);
    Task<CustomerDto> GetCustomerAsync(int id, CancellationToken token);
    Task<CustomerDto> UpdateCustomerAsync(int id, CustomerUpdateRequest request, CancellationToken token);
    Task<CustomerDto> SetCustomerStatusAsync(int id, bool active, CancellationToken token);
    Task<PagedResult<AccountDto>> GetAccountsAsync(string? search, int page, int pageSize, CancellationToken token);
    Task<AccountDto> CreateAccountAsync(AccountRequest request, CancellationToken token);
    Task<AccountDto> GetAccountAsync(int id, CancellationToken token);
    Task<AccountDto> SetAccountStatusAsync(int id, bool active, CancellationToken token);
    Task<MoneyResult> DepositAsync(MoneyRequest request, CancellationToken token);
    Task<MoneyResult> WithdrawAsync(MoneyRequest request, CancellationToken token);
    Task<TransferResult> TransferAsync(TransferRequest request, CancellationToken token);
    Task<PagedResult<TransactionDto>> GetTransactionsAsync(int? accountId, string? type, int page, int pageSize, CancellationToken token);
}

public record MoneyResult(string ReferenceNumber, string AccountNumber, decimal NewBalance);
public record TransferResult(string ReferenceNumber, string FromAccountNumber, decimal FromBalance, string ToAccountNumber, decimal ToBalance);
