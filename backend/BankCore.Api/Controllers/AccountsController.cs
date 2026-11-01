using BankCore.Api.DTOs;
using BankCore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankCore.Api.Controllers;

[ApiController, Route("api/accounts")]
public class AccountsController(IBankingService service) : ControllerBase
{
    [HttpGet] public Task<PagedResult<AccountDto>> GetAll(string? search, int page = 1, int pageSize = 20, CancellationToken token = default) =>
        service.GetAccountsAsync(search, Math.Max(1, page), Math.Clamp(pageSize, 1, 100), token);
    [HttpGet("{id:int}")] public Task<AccountDto> Get(int id, CancellationToken token) => service.GetAccountAsync(id, token);
    [HttpPost] public async Task<ActionResult<AccountDto>> Create(AccountRequest request, CancellationToken token)
    {
        var result = await service.CreateAccountAsync(request, token);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPatch("{id:int}/status")] public Task<AccountDto> SetStatus(int id, [FromQuery] bool active, CancellationToken token) =>
        service.SetAccountStatusAsync(id, active, token);
}
