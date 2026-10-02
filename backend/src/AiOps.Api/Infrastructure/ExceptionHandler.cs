using AiOps.Application.Common;
using AiOps.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AiOps.Api.Infrastructure;

/// <summary>Maps known exceptions to RFC 7807 problem responses.</summary>
public sealed class ExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            DomainException => (StatusCodes.Status409Conflict, "Request conflicts with job state"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
            _ => (0, string.Empty),
        };
        if (status == 0) return false;

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = exception.Message },
        });
    }
}
