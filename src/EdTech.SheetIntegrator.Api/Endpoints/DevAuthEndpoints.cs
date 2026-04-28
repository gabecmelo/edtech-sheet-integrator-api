using EdTech.SheetIntegrator.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EdTech.SheetIntegrator.Api.Endpoints;

/// <summary>
/// Dev-only token issuance. Mounted at <c>/dev/token</c> only when the host environment is
/// <c>Development</c>. In production the API expects tokens minted by an external IdP.
/// </summary>
internal static class DevAuthEndpoints
{
    public static IEndpointRouteBuilder MapDevAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dev/token", IssueToken)
            .WithName("IssueDevToken")
            .WithSummary("Mint an Instructor-role JWT for local manual testing (Development only)")
            .Produces<DevTokenResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static IResult IssueToken(
        [FromBody] DevTokenRequest? request,
        [FromServices] JwtTokenService tokenService)
    {
        var subject = string.IsNullOrWhiteSpace(request?.Subject) ? "dev-instructor" : request.Subject;
        var token = tokenService.IssueInstructorToken(subject);
        return Results.Ok(new DevTokenResponse(token, subject));
    }
}

internal sealed record DevTokenRequest(string? Subject);

internal sealed record DevTokenResponse(string Token, string Subject);
