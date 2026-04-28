using EdTech.SheetIntegrator.Application.Assessments.Dtos;
using EdTech.SheetIntegrator.Domain.Grading;
using FluentValidation;

namespace EdTech.SheetIntegrator.Application.Assessments.Validators;

public sealed class CreateAssessmentRequestValidator : AbstractValidator<CreateAssessmentRequest>
{
    public CreateAssessmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Questions)
            .NotNull()
            .Must(q => q.Count > 0).WithMessage("At least one question is required.");

        RuleForEach(x => x.Questions).SetValidator(new QuestionInputValidator());
    }
}

internal sealed class QuestionInputValidator : AbstractValidator<QuestionInput>
{
    public QuestionInputValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.CorrectAnswer).NotNull().MaximumLength(2000);
        RuleFor(x => x.Points).GreaterThan(0m);
        RuleFor(x => x.MatchMode).IsInEnum();

        When(x => x.MatchMode == MatchMode.Numeric, () =>
        {
            RuleFor(x => x.NumericTolerance)
                .NotNull()
                .GreaterThanOrEqualTo(0m)
                .WithMessage("Numeric questions require a non-negative tolerance.");
        });

        When(x => x.MatchMode != MatchMode.Numeric, () =>
        {
            RuleFor(x => x.NumericTolerance)
                .Null()
                .WithMessage("Numeric tolerance is only valid for Numeric match mode.");
        });
    }
}
