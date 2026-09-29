using FinTrack.Application.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[Route("api/dashboard")]
public class DashboardController(TransactionService service) : AuthenticatedController
{
    [HttpGet]
    public Task<FinancialSummary> Get([FromQuery] TransactionFilter filter, CancellationToken ct) => service.Summary(UserId, filter, ct);
}
