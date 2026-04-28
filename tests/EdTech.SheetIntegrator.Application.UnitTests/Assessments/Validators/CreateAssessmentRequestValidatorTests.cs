using EdTech.SheetIntegrator.Application.Assessments.Validators;
using EdTech.SheetIntegrator.Application.UnitTests.TestData;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Application.UnitTests.Assessments.Validators;

public class CreateAssessmentRequestValidatorTests
{
    private readonly CreateAssessmentRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var req = AppFixtures.CreateAssessmentRequest();

        var result = _validator.Validate(req);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Title_Fails()
    {
        var req = AppFixtures.CreateAssessmentRequest(title: "");

        var result = _validator.Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Empty_Questions_List_Fails()
    {
        var req = AppFixtures.CreateAssessmentRequest(questions: []);

        var result = _validator.Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Questions");
    }

    [Fact]
    public void Numeric_Question_Without_Tolerance_Fails()
    {
        var bad = new Application.Assessments.Dtos.QuestionInput(
            "Q1", "Pi?", "3.14", 1m, MatchMode.Numeric, NumericTolerance: null);
        var req = AppFixtures.CreateAssessmentRequest(questions: [bad]);

        var result = _validator.Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("tolerance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Exact_Question_With_Tolerance_Fails()
    {
        var bad = new Application.Assessments.Dtos.QuestionInput(
            "Q1", "p", "Paris", 1m, MatchMode.Exact, NumericTolerance: 0.5m);
        var req = AppFixtures.CreateAssessmentRequest(questions: [bad]);

        var result = _validator.Validate(req);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Numeric", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Non_Positive_Points_Fails()
    {
        var bad = new Application.Assessments.Dtos.QuestionInput(
            "Q1", "p", "Paris", 0m, MatchMode.Exact, NumericTolerance: null);
        var req = AppFixtures.CreateAssessmentRequest(questions: [bad]);

        var result = _validator.Validate(req);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Undefined_MatchMode_Enum_Fails()
    {
        var bad = new Application.Assessments.Dtos.QuestionInput(
            "Q1", "p", "Paris", 1m, (MatchMode)999, NumericTolerance: null);
        var req = AppFixtures.CreateAssessmentRequest(questions: [bad]);

        var result = _validator.Validate(req);

        result.IsValid.Should().BeFalse();
    }
}
