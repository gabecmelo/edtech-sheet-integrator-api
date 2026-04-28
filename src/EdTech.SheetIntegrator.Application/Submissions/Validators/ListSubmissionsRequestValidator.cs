using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using FluentValidation;

namespace EdTech.SheetIntegrator.Application.Submissions.Validators;

public sealed class ListSubmissionsRequestValidator : AbstractValidator<ListSubmissionsRequest>
{
    public ListSubmissionsRequestValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
