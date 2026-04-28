using EdTech.SheetIntegrator.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EdTech.SheetIntegrator.Api.ErrorMapping;

/// <summary>
/// Maps an Application-layer <see cref="Result{T}"/> into an HTTP response. Each stable error
/// code defined in <see cref="Errors"/> maps to a specific RFC 7807 problem-details status:
/// <list type="bullet">
///   <item><c>validation.failed</c> -&gt; 400 Bad Request</item>
///   <item><c>*.not_found</c> -&gt; 404 Not Found</item>
///   <item><c>submission.unsupported_file_type</c> -&gt; 415 Unsupported Media Type</item>
///   <item><c>submission.unreadable_sheet</c> / <c>submission.mismatched_answer_sheet</c> /
///         <c>submission.invalid</c> / <c>assessment.invalid_configuration</c> -&gt; 422 Unprocessable Entity</item>
///   <item>anything else -&gt; 500 Internal Server Error</item>
/// </list>
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is null
                ? Results.Ok(result.Value)
                : onSuccess(result.Value);
        }

        return ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error)
    {
        var (status, title) = error.Code switch
        {
            "validation.failed" => (StatusCodes.Status400BadRequest, "Validation failed"),
            "assessment.not_found" or "submission.not_found" => (StatusCodes.Status404NotFound, "Resource not found"),
            "submission.unsupported_file_type" => (StatusCodes.Status415UnsupportedMediaType, "Unsupported file type"),
            "submission.unreadable_sheet"
                or "submission.mismatched_answer_sheet"
                or "submission.invalid"
                or "assessment.invalid_configuration"
                => (StatusCodes.Status422UnprocessableEntity, "Cannot process the request"),
            _ => (StatusCodes.Status500InternalServerError, "Server error"),
        };

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: error.Message,
            type: $"https://edtech-sheet-integrator/errors/{error.Code}",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code,
            });
    }
}
