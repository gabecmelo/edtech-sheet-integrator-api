using EdTech.SheetIntegrator.Api.Auth;
using EdTech.SheetIntegrator.Api.ErrorMapping;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EdTech.SheetIntegrator.Api.Endpoints;

internal static class SubmissionsEndpoints
{
    public static RouteGroupBuilder MapAssessmentSubmissionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/{assessmentId:guid}/submissions", SubmitGradedSheet)
            .WithName("SubmitGradedSheet")
            .WithSummary("Upload a student's answer sheet (xlsx or csv) and grade it against the assessment")
            .RequireAuthorization(JwtTokenService.InstructorPolicy)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<SubmissionResultResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status415UnsupportedMediaType)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{assessmentId:guid}/submissions", ListSubmissions)
            .WithName("ListSubmissionsForAssessment")
            .WithSummary("Page through submissions for an assessment, newest first")
            .Produces<PagedResult<SubmissionResultResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    public static RouteGroupBuilder MapSubmissionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", GetSubmissionResult)
            .WithName("GetSubmissionResult")
            .WithSummary("Fetch a single submission and its grading result")
            .Produces<SubmissionResultResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> SubmitGradedSheet(
        [FromRoute] Guid assessmentId,
        [FromForm] string studentIdentifier,
        IFormFile file,
        [FromServices] IUseCase<SubmitGradedSheetRequest, SubmissionResultResponse> useCase,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var request = new SubmitGradedSheetRequest(
            assessmentId,
            studentIdentifier,
            file.FileName,
            file.ContentType,
            stream);

        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult(value =>
            Results.CreatedAtRoute("GetSubmissionResult", new { id = value.Id }, value));
    }

    private static async Task<IResult> ListSubmissions(
        [FromRoute] Guid assessmentId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IUseCase<ListSubmissionsRequest, PagedResult<SubmissionResultResponse>> useCase,
        CancellationToken cancellationToken)
    {
        var request = new ListSubmissionsRequest(
            assessmentId,
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize);

        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetSubmissionResult(
        Guid id,
        [FromServices] IUseCase<GetSubmissionResultRequest, SubmissionResultResponse> useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetSubmissionResultRequest(id), cancellationToken);
        return result.ToHttpResult();
    }
}
