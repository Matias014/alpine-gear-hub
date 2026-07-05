using AlpineGearHub.Identity.Domain.Exceptions;
using AlpineGearHub.Listings.Domain.Exceptions;
using AlpineGearHub.SharedKernel.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AlpineGearHub.Api.Middleware;

/// <summary>
/// Maps well-known exceptions to consistent ProblemDetails responses (RFC 9457).
///   ValidationException                     → 422 Unprocessable Entity
///   EmailAlreadyTakenException              → 409 Conflict
///   InvalidCredentialsException             → 401 Unauthorized
///   InvalidRefreshTokenException            → 401 Unauthorized
///   TooManyLoginAttemptsException           → 429 Too Many Requests
///   UnauthorizedAccessException             → 403 Forbidden
///   InvalidOperationException (not found)  → 404 Not Found
///   InvalidListingStatusTransitionException → 422 Unprocessable Entity
///   BadHttpRequestException (invalid JSON)  → 400 Bad Request
///   DomainException (other)                → 422 Unprocessable Entity
///   StripeException                        → 502 Bad Gateway
///   Everything else                        → 500 Internal Server Error
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            EmailAlreadyTakenException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),

            InvalidCredentialsException or InvalidRefreshTokenException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message),

            TooManyLoginAttemptsException => (
                StatusCodes.Status429TooManyRequests,
                "Too many requests",
                exception.Message),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message),

            InvalidListingStatusTransitionException => (
                StatusCodes.Status422UnprocessableEntity,
                "Invalid status transition",
                exception.Message),

            InvalidOperationException => (
                StatusCodes.Status404NotFound,
                "Not found",
                exception.Message),

            BadHttpRequestException badRequest => (
                StatusCodes.Status400BadRequest,
                "Bad request",
                badRequest.Message),

            DomainException => (
                StatusCodes.Status422UnprocessableEntity,
                "Business rule violation",
                exception.Message),

            // The payment provider itself failed (bad/placeholder API key, card decline forwarded
            // as an exception, transient Stripe outage) - not something the caller can fix by
            // retrying with different input, so 502 (upstream failure) fits better than a plain 500.
            StripeException => (
                StatusCodes.Status502BadGateway,
                "Payment provider error",
                "The payment provider could not process this request. Please try again shortly."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An unexpected error occurred. Please try again later.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            logger.LogWarning(exception, "Handled exception ({Status}): {Message}", statusCode, exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
