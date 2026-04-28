using EdTech.SheetIntegrator.Api.Auth;
using EdTech.SheetIntegrator.Api.ErrorMapping;
using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace EdTech.SheetIntegrator.Api.Endpoints;

internal static class AssessmentsEndpoints
{
    public static RouteGroupBuilder MapAssessmentsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateAssessment)
            .WithName("CreateAssessment")
            .WithSummary("Create a new assessment with its answer key")
            .RequireAuthorization(JwtTokenService.InstructorPolicy)
            .Produces<AssessmentResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", GetAssessmentById)
            .WithName("GetAssessmentById")
            .WithSummary("Fetch an assessment definition")
            .Produces<AssessmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> CreateAssessment(
        [FromBody] CreateAssessmentRequest request,
        [FromServices] IUseCase<CreateAssessmentRequest, AssessmentResponse> useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult(value =>
            Results.CreatedAtRoute("GetAssessmentById", new { id = value.Id }, value));
    }

    private static async Task<IResult> GetAssessmentById(
        Guid id,
        [FromServices] IUseCase<GetAssessmentByIdRequest, AssessmentResponse> useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetAssessmentByIdRequest(id), cancellationToken);
        return result.ToHttpResult();
    }
}
