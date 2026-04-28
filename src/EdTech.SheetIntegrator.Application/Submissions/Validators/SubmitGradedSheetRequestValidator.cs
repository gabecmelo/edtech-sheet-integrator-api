using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using FluentValidation;

namespace EdTech.SheetIntegrator.Application.Submissions.Validators;

public sealed class SubmitGradedSheetRequestValidator : AbstractValidator<SubmitGradedSheetRequest>
{
    public SubmitGradedSheetRequestValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
        RuleFor(x => x.StudentIdentifier).NotEmpty().MaximumLength(256);
        RuleFor(x => x.SourceFileName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.FileStream).NotNull();
    }
}
