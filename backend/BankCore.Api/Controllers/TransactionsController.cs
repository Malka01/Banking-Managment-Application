using BankCore.Api.DTOs;
using BankCore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankCore.Api.Controllers;

[ApiController, Route("api/transactions")]
public class TransactionsController(IBankingService service) : ControllerBase
{
    [HttpPost("deposit")] public Task<MoneyResult> Deposit(MoneyRequest request, CancellationToken token) => service.DepositAsync(request, token);
    [HttpPost("withdraw")] public Task<MoneyResult> Withdraw(MoneyRequest request, CancellationToken token) => service.WithdrawAsync(request, token);
    [HttpPost("transfer")] public Task<TransferResult> Transfer(TransferRequest request, CancellationToken token) => service.TransferAsync(request, token);
    [HttpGet] public Task<PagedResult<TransactionDto>> GetAll(int? accountId, string? type, int page = 1, int pageSize = 25, CancellationToken token = default) =>
        service.GetTransactionsAsync(accountId, type, Math.Max(1, page), Math.Clamp(pageSize, 1, 100), token);
}
