using EdTech.SheetIntegrator.Application.Assessments.UseCases;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Application.Submissions.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EdTech.SheetIntegrator.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the use cases and FluentValidation validators defined in the Application assembly.
    /// Infrastructure registrations (repositories, parsers, clock, unit-of-work) are added separately
    /// by the Infrastructure layer's own DI extension.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>(ServiceLifetime.Scoped);

        services.AddScoped<IUseCase<
            Assessments.Dtos.CreateAssessmentRequest,
            Assessments.Dtos.AssessmentResponse>, CreateAssessmentUseCase>();
        services.AddScoped<IUseCase<
            Assessments.Dtos.GetAssessmentByIdRequest,
            Assessments.Dtos.AssessmentResponse>, GetAssessmentByIdUseCase>();

        services.AddScoped<IUseCase<
            Submissions.Dtos.SubmitGradedSheetRequest,
            Submissions.Dtos.SubmissionResultResponse>, SubmitGradedSheetUseCase>();
        services.AddScoped<IUseCase<
            Submissions.Dtos.GetSubmissionResultRequest,
            Submissions.Dtos.SubmissionResultResponse>, GetSubmissionResultUseCase>();
        services.AddScoped<IUseCase<
            Submissions.Dtos.ListSubmissionsRequest,
            PagedResult<Submissions.Dtos.SubmissionResultResponse>>, ListSubmissionsForAssessmentUseCase>();

        return services;
    }
}

/// <summary>Marker for assembly-scanning APIs that need a stable type from this assembly.</summary>
public interface IApplicationAssemblyMarker
{
}
