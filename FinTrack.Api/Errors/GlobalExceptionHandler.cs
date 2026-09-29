using System.ComponentModel.DataAnnotations;
using FinTrack.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Errors;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ArgumentException or ValidationException => 400,
            AuthenticationException => 401,
            NotFoundException => 404,
            ConflictException => 409,
            _ => 500
        };
        if (status == 500) logger.LogError(exception, "Erro não tratado. TraceId: {TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status == 500 ? "Ocorreu um erro interno." : exception.Message,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, cancellationToken);
        return true;
    }
}
