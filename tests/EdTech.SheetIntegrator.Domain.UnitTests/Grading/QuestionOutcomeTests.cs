using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Grading;

public class QuestionOutcomeTests
{
    [Fact]
    public void Constructor_Sets_Properties_For_Correct_Outcome()
    {
        var outcome = new QuestionOutcome("Q1", isCorrect: true, earnedPoints: 5m, maxPoints: 5m);

        outcome.QuestionId.Should().Be("Q1");
        outcome.IsCorrect.Should().BeTrue();
        outcome.EarnedPoints.Should().Be(5m);
        outcome.MaxPoints.Should().Be(5m);
    }

    [Fact]
    public void Constructor_Allows_Zero_Earned_Points()
    {
        var outcome = new QuestionOutcome("Q1", isCorrect: false, earnedPoints: 0m, maxPoints: 5m);

        outcome.IsCorrect.Should().BeFalse();
        outcome.EarnedPoints.Should().Be(0m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_When_QuestionId_Is_Blank(string? questionId)
    {
        var act = () => new QuestionOutcome(questionId!, isCorrect: false, earnedPoints: 0m, maxPoints: 1m);

        act.Should().Throw<DomainException>().WithMessage("*question id*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_When_MaxPoints_Is_Not_Positive(double maxPoints)
    {
        var act = () => new QuestionOutcome("Q1", isCorrect: false, earnedPoints: 0m, maxPoints: (decimal)maxPoints);

        act.Should().Throw<DomainException>().WithMessage("*max points*");
    }

    [Fact]
    public void Constructor_Throws_When_EarnedPoints_Is_Negative()
    {
        var act = () => new QuestionOutcome("Q1", isCorrect: false, earnedPoints: -0.01m, maxPoints: 5m);

        act.Should().Throw<DomainException>().WithMessage("*negative*");
    }

    [Fact]
    public void Constructor_Throws_When_EarnedPoints_Exceeds_MaxPoints()
    {
        var act = () => new QuestionOutcome("Q1", isCorrect: true, earnedPoints: 5.01m, maxPoints: 5m);

        act.Should().Throw<DomainException>().WithMessage("*exceed*");
    }
}
