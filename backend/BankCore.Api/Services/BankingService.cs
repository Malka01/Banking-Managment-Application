using BankCore.Api.Data;
using BankCore.Api.DTOs;
using BankCore.Api.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Services;

public sealed class BankingService(BankCoreDbContext db, ILogger<BankingService> logger) : IBankingService
{
    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(string? search, int page, int pageSize, CancellationToken token)
    {
        var query = db.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.FullName.Contains(search) || x.Nic.Contains(search) || x.Email.Contains(search));
        var total = await query.CountAsync(token);
        var items = await query.OrderBy(x => x.FullName).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new CustomerDto(x.Id, x.FullName, x.Nic, x.Email, x.PhoneNumber, x.Address, x.IsActive, x.CreatedAt)).ToListAsync(token);
        return new(items, page, pageSize, total);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerRequest request, CancellationToken token)
    {
        if (await db.Customers.AnyAsync(x => x.Nic == request.Nic, token)) throw new InvalidOperationException("NIC is already registered.");
        var customer = new Customer { FullName = request.FullName.Trim(), Nic = request.Nic.Trim(), Email = request.Email.Trim(), PhoneNumber = request.PhoneNumber.Trim(), Address = request.Address.Trim() };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(token);
        return ToDto(customer);
    }

    public async Task<CustomerDto> GetCustomerAsync(int id, CancellationToken token)
    {
        var customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, token) ?? throw new KeyNotFoundException("Customer was not found.");
        return ToDto(customer);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerUpdateRequest request, CancellationToken token)
    {
        var customer = await db.Customers.FindAsync([id], token) ?? throw new KeyNotFoundException("Customer was not found.");
        customer.FullName = request.FullName.Trim();
        customer.Email = request.Email.Trim();
        customer.PhoneNumber = request.PhoneNumber.Trim();
        customer.Address = request.Address.Trim();
        await db.SaveChangesAsync(token);
        return ToDto(customer);
    }

    public async Task<CustomerDto> SetCustomerStatusAsync(int id, bool active, CancellationToken token)
    {
        var customer = await db.Customers.FindAsync([id], token) ?? throw new KeyNotFoundException("Customer was not found.");
        customer.IsActive = active;
        await db.SaveChangesAsync(token);
        return ToDto(customer);
    }

    public async Task<PagedResult<AccountDto>> GetAccountsAsync(string? search, int page, int pageSize, CancellationToken token)
    {
        var query = db.Accounts.AsNoTracking().Include(x => x.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.AccountNumber.Contains(search) || x.Customer.FullName.Contains(search));
        var total = await query.CountAsync(token);
        var items = await query.OrderBy(x => x.AccountNumber).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AccountDto(x.Id, x.AccountNumber, x.CustomerId, x.Customer.FullName, x.AccountType, x.Balance, x.Currency, x.IsActive, x.CreatedAt)).ToListAsync(token);
        return new(items, page, pageSize, total);
    }

    public async Task<AccountDto> CreateAccountAsync(AccountRequest request, CancellationToken token)
    {
        if (await db.Accounts.AnyAsync(x => x.AccountNumber == request.AccountNumber, token)) throw new InvalidOperationException("Account number is already in use.");
        var customer = await db.Customers.FindAsync([request.CustomerId], token) ?? throw new KeyNotFoundException("Customer was not found.");
        if (!customer.IsActive) throw new InvalidOperationException("Cannot open an account for an inactive customer.");
        var account = new Account { AccountNumber = request.AccountNumber.Trim(), CustomerId = request.CustomerId, AccountType = request.AccountType.Trim(), Balance = request.OpeningBalance, Currency = request.Currency.ToUpperInvariant() };
        db.Accounts.Add(account);
        await db.SaveChangesAsync(token);
        return new(account.Id, account.AccountNumber, account.CustomerId, customer.FullName, account.AccountType, account.Balance, account.Currency, account.IsActive, account.CreatedAt);
    }

    public async Task<AccountDto> GetAccountAsync(int id, CancellationToken token)
    {
        var account = await db.Accounts.AsNoTracking().Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id, token) ?? throw new KeyNotFoundException("Account was not found.");
        return ToDto(account);
    }

    public async Task<AccountDto> SetAccountStatusAsync(int id, bool active, CancellationToken token)
    {
        var account = await db.Accounts.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id, token)
            ?? throw new KeyNotFoundException("Account was not found.");
        account.IsActive = active;
        await db.SaveChangesAsync(token);
        return ToDto(account);
    }

    public Task<MoneyResult> DepositAsync(MoneyRequest request, CancellationToken token) => ApplyMoneyAsync(request, false, token);
    public Task<MoneyResult> WithdrawAsync(MoneyRequest request, CancellationToken token) => ApplyMoneyAsync(request, true, token);

    private async Task<MoneyResult> ApplyMoneyAsync(MoneyRequest request, bool withdrawal, CancellationToken token)
    {
        ValidateAmount(request.Amount);
        var accountNumber = NormalizeAccountNumber(request.AccountNumber);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        try
        {
            var account = await db.Accounts.SingleOrDefaultAsync(x => x.AccountNumber == accountNumber, token)
                ?? throw new KeyNotFoundException("Account was not found.");
            EnsureActive(account);
            if (withdrawal && account.Balance < request.Amount) throw new InvalidOperationException("Insufficient balance.");

            var previous = account.Balance;
            account.Balance += withdrawal ? -request.Amount : request.Amount;
            var reference = CreateReference();
            var type = withdrawal ? "WITHDRAWAL" : "DEPOSIT";
            db.Transactions.Add(new Transaction { ReferenceNumber = reference, AccountId = account.Id, Type = type, Amount = request.Amount, PreviousBalance = previous, NewBalance = account.Balance, Description = request.Description });
            db.AuditLogs.Add(new AuditLog { Action = type, EntityName = "Account", EntityId = account.Id, ReferenceNumber = reference, Details = request.Description });
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            logger.LogInformation("{Type} completed for account {AccountNumber} with reference {Reference}", type, account.AccountNumber, reference);
            return new(reference, account.AccountNumber, account.Balance);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new InvalidOperationException("The account changed during this transaction. Please retry.", exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<TransferResult> TransferAsync(TransferRequest request, CancellationToken token)
    {
        ValidateAmount(request.Amount);
        var fromNumber = NormalizeAccountNumber(request.FromAccountNumber);
        var toNumber = NormalizeAccountNumber(request.ToAccountNumber);
        if (fromNumber == toNumber) throw new InvalidOperationException("Source and destination accounts must be different.");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        try
        {
            var accounts = await db.Accounts.Where(x => x.AccountNumber == fromNumber || x.AccountNumber == toNumber)
                .OrderBy(x => x.Id).ToListAsync(token);
            var from = accounts.SingleOrDefault(x => x.AccountNumber == fromNumber) ?? throw new KeyNotFoundException("Source account was not found.");
            var to = accounts.SingleOrDefault(x => x.AccountNumber == toNumber) ?? throw new KeyNotFoundException("Destination account was not found.");
            EnsureActive(from);
            EnsureActive(to);
            if (from.Balance < request.Amount) throw new InvalidOperationException("Insufficient balance.");

            var reference = CreateReference();
            var fromPrevious = from.Balance;
            var toPrevious = to.Balance;
            from.Balance -= request.Amount;
            to.Balance += request.Amount;
            db.Transactions.AddRange(
                new Transaction { ReferenceNumber = reference, AccountId = from.Id, Type = "TRANSFER_OUT", Amount = request.Amount, PreviousBalance = fromPrevious, NewBalance = from.Balance, Description = request.Description },
                new Transaction { ReferenceNumber = reference, AccountId = to.Id, Type = "TRANSFER_IN", Amount = request.Amount, PreviousBalance = toPrevious, NewBalance = to.Balance, Description = request.Description });
            db.AuditLogs.Add(new AuditLog { Action = "TRANSFER", EntityName = "Account", EntityId = from.Id, ReferenceNumber = reference, Details = request.Description });
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            logger.LogInformation("Transfer completed from {FromAccount} to {ToAccount} with reference {Reference}", from.AccountNumber, to.AccountNumber, reference);
            return new(reference, from.AccountNumber, from.Balance, to.AccountNumber, to.Balance);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new InvalidOperationException("One of the accounts changed during this transfer. Please retry.", exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(int? accountId, string? type, int page, int pageSize, CancellationToken token)
    {
        var query = db.Transactions.AsNoTracking().Include(x => x.Account).AsQueryable();
        if (accountId.HasValue) query = query.Where(x => x.AccountId == accountId);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.Type == type.ToUpper());
        var total = await query.CountAsync(token);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new TransactionDto(x.Id, x.ReferenceNumber, x.AccountId, x.Account.AccountNumber, x.Type, x.Amount, x.PreviousBalance, x.NewBalance, x.Description, x.CreatedAt)).ToListAsync(token);
        return new(items, page, pageSize, total);
    }

    private static CustomerDto ToDto(Customer x) => new(x.Id, x.FullName, x.Nic, x.Email, x.PhoneNumber, x.Address, x.IsActive, x.CreatedAt);
    private static AccountDto ToDto(Account x) => new(x.Id, x.AccountNumber, x.CustomerId, x.Customer.FullName, x.AccountType, x.Balance, x.Currency, x.IsActive, x.CreatedAt);
    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be greater than zero.");
    }

    private static void EnsureActive(Account account)
    {
        if (!account.IsActive) throw new InvalidOperationException("Account is inactive.");
    }

    private static string NormalizeAccountNumber(string accountNumber) => accountNumber.Trim();
    private static string CreateReference() => $"TX-{Guid.NewGuid():N}".ToUpperInvariant();
}
