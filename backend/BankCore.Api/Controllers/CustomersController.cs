using BankCore.Api.DTOs;
using BankCore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankCore.Api.Controllers;

[ApiController, Route("api/customers")]
public class CustomersController(IBankingService service) : ControllerBase
{
    [HttpGet] public Task<PagedResult<CustomerDto>> GetAll(string? search, int page = 1, int pageSize = 20, CancellationToken token = default) =>
        service.GetCustomersAsync(search, Math.Max(1, page), Math.Clamp(pageSize, 1, 100), token);
    [HttpGet("{id:int}")] public Task<CustomerDto> Get(int id, CancellationToken token) => service.GetCustomerAsync(id, token);
    [HttpPut("{id:int}")]
    public Task<CustomerDto> Update(int id, CustomerUpdateRequest request, CancellationToken token) =>
        service.UpdateCustomerAsync(id, request, token);
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerRequest request, CancellationToken token)
    {
        var result = await service.CreateCustomerAsync(request, token);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPatch("{id:int}/status")] public Task<CustomerDto> SetStatus(int id, [FromQuery] bool active, CancellationToken token) => service.SetCustomerStatusAsync(id, active, token);
}
