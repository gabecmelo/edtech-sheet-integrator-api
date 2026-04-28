using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EdTech.SheetIntegrator.Application.Assessments.UseCases;

public sealed class CreateAssessmentUseCase : IUseCase<CreateAssessmentRequest, AssessmentResponse>
{
    private readonly IAssessmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IValidator<CreateAssessmentRequest> _validator;
    private readonly ILogger<CreateAssessmentUseCase> _logger;

    public CreateAssessmentUseCase(
        IAssessmentRepository repository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IValidator<CreateAssessmentRequest> validator,
        ILogger<CreateAssessmentUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AssessmentResponse>> ExecuteAsync(
        CreateAssessmentRequest input,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Errors.Validation.Failed(validation.Errors.Select(e => e.ErrorMessage));
        }

        Assessment assessment;
        try
        {
            var questions = input.Questions
                .Select(q => new Question(q.QuestionId, q.Prompt, q.CorrectAnswer, q.Points, q.MatchMode, q.NumericTolerance))
                .ToList();
            assessment = new Assessment(Guid.NewGuid(), input.Title, questions, _clock.UtcNow);
        }
        catch (DomainException ex)
        {
            _logger.LogInformation(ex, "Assessment creation rejected by domain invariants.");
            return Errors.Assessment.InvalidConfiguration(ex.Message);
        }

        await _repository.AddAsync(assessment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created assessment {AssessmentId} with {QuestionCount} questions.",
            assessment.Id, assessment.Questions.Count);

        return AssessmentResponse.From(assessment);
    }
}
