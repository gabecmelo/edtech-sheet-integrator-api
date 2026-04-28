using EdTech.SheetIntegrator.Application.Abstractions.Persistence;
using EdTech.SheetIntegrator.Application.Abstractions.Sheets;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Submissions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EdTech.SheetIntegrator.Application.Submissions.UseCases;

/// <summary>
/// Core feature: receive a sheet upload, parse it, run grading against the assessment's answer key,
/// persist the graded submission. Each step maps a specific failure to a stable
/// <see cref="Error"/> code so the API can translate it into the right HTTP status.
/// </summary>
public sealed class SubmitGradedSheetUseCase : IUseCase<SubmitGradedSheetRequest, SubmissionResultResponse>
{
    private readonly IAssessmentRepository _assessments;
    private readonly ISubmissionRepository _submissions;
    private readonly ISheetParserFactory _parserFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IValidator<SubmitGradedSheetRequest> _validator;
    private readonly ILogger<SubmitGradedSheetUseCase> _logger;

    public SubmitGradedSheetUseCase(
        IAssessmentRepository assessments,
        ISubmissionRepository submissions,
        ISheetParserFactory parserFactory,
        IUnitOfWork unitOfWork,
        IClock clock,
        IValidator<SubmitGradedSheetRequest> validator,
        ILogger<SubmitGradedSheetUseCase> logger)
    {
        _assessments = assessments;
        _submissions = submissions;
        _parserFactory = parserFactory;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<SubmissionResultResponse>> ExecuteAsync(
        SubmitGradedSheetRequest input,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Errors.Validation.Failed(validation.Errors.Select(e => e.ErrorMessage));
        }

        var assessment = await _assessments.GetByIdAsync(input.AssessmentId, cancellationToken);
        if (assessment is null)
        {
            return Errors.Assessment.NotFound;
        }

        var parser = _parserFactory.Resolve(input.SourceFileName, input.ContentType);
        if (parser is null)
        {
            return Errors.Submission.UnsupportedFileType(
                $"No parser available for '{input.SourceFileName}'.");
        }

        IReadOnlyList<RawAnswer> rawAnswers;
        try
        {
            rawAnswers = await parser.ParseAsync(input.FileStream, cancellationToken);
        }
        catch (SheetParsingException ex)
        {
            _logger.LogInformation(ex, "Sheet parser rejected upload {FileName}.", input.SourceFileName);
            return Errors.Submission.UnreadableSheet(ex.Message);
        }

        StudentSubmission submission;
        try
        {
            var answers = rawAnswers.Select(a => new Answer(a.QuestionId, a.Response)).ToList();
            submission = new StudentSubmission(
                Guid.NewGuid(),
                assessment.Id,
                input.StudentIdentifier,
                answers,
                input.SourceFileName,
                _clock.UtcNow);
        }
        catch (DomainException ex)
        {
            _logger.LogInformation(ex, "Submission rejected by domain invariants.");
            return Errors.Submission.Invalid(ex.Message);
        }

        try
        {
            var gradingResult = assessment.Grade(submission.Answers, _clock.UtcNow);
            submission.AttachResult(gradingResult);
        }
        catch (MismatchedAnswerSheetException ex)
        {
            _logger.LogInformation(ex, "Submission referenced unknown questions.");
            return Errors.Submission.MismatchedAnswerSheet(ex.Message);
        }

        await _submissions.AddAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Graded submission {SubmissionId} for assessment {AssessmentId}: {Earned}/{Total} ({Percentage}%).",
            submission.Id,
            assessment.Id,
            submission.Result!.Score.Earned,
            submission.Result.Score.Total,
            submission.Result.Score.Percentage);

        return SubmissionResultResponse.From(submission);
    }
}
