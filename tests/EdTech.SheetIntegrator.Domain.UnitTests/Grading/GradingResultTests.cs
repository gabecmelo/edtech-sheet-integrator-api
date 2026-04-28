using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.UnitTests.TestData;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Grading;

public class GradingResultTests
{
    private static readonly QuestionOutcome Q1Correct = new("Q1", true, 1m, 1m);
    private static readonly QuestionOutcome Q2Wrong = new("Q2", false, 0m, 2m);

    [Fact]
    public void Constructor_Sets_Properties_When_Sums_Are_Consistent()
    {
        var score = new Score(1m, 3m);
        var outcomes = new[] { Q1Correct, Q2Wrong };

        var result = new GradingResult(score, outcomes, Fixtures.Now);

        result.Score.Should().Be(score);
        result.Outcomes.Should().BeEquivalentTo(outcomes);
        result.GradedAt.Should().Be(Fixtures.Now);
    }

    [Fact]
    public void Constructor_Throws_When_Outcomes_Is_Empty()
    {
        var act = () => new GradingResult(new Score(0m, 1m), [], Fixtures.Now);

        act.Should().Throw<DomainException>().WithMessage("*at least one*");
    }

    [Fact]
    public void Constructor_Throws_When_Outcomes_Max_Points_Disagree_With_Score_Total()
    {
        // Outcomes total = 1 + 2 = 3, but Score.Total = 5
        var act = () => new GradingResult(
            new Score(1m, 5m),
            [Q1Correct, Q2Wrong],
            Fixtures.Now);

        act.Should().Throw<DomainException>().WithMessage("*max points*total*");
    }

    [Fact]
    public void Constructor_Throws_When_Outcomes_Earned_Disagree_With_Score_Earned()
    {
        // Outcomes earned = 1, but Score.Earned = 0
        var act = () => new GradingResult(
            new Score(0m, 3m),
            [Q1Correct, Q2Wrong],
            Fixtures.Now);

        act.Should().Throw<DomainException>().WithMessage("*earned points*");
    }
}
