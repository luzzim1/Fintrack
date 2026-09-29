using FinTrack.Application.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[Route("api/transactions")]
public class TransactionsController(TransactionService service) : AuthenticatedController
{
    [HttpGet]
    public Task<Page<TransactionResponse>> List([FromQuery] TransactionFilter filter, CancellationToken ct) => service.List(UserId, filter, ct);
    [HttpGet("{id:guid}")]
    public Task<TransactionResponse> Get(Guid id, CancellationToken ct) => service.Get(UserId, id, ct);
    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(TransactionRequest request, CancellationToken ct)
    {
        var transaction = await service.Create(UserId, request, ct);
        return CreatedAtAction(nameof(Get), new { id = transaction.Id }, transaction);
    }
    [HttpPut("{id:guid}")]
    public Task<TransactionResponse> Update(Guid id, TransactionRequest request, CancellationToken ct) => service.Update(UserId, id, request, ct);
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await service.Delete(UserId, id, ct); return NoContent(); }
}
