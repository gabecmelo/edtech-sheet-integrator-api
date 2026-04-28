using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Grading;

public class ScoreTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(10, 10)]
    [InlineData(0.5, 1.0)]
    public void Constructor_Accepts_Values_Within_Invariants(double earned, double total)
    {
        var score = new Score((decimal)earned, (decimal)total);

        score.Earned.Should().Be((decimal)earned);
        score.Total.Should().Be((decimal)total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1)]
    public void Constructor_Throws_When_Total_Is_Not_Positive(double total)
    {
        var act = () => new Score(0m, (decimal)total);

        act.Should().Throw<DomainException>().WithMessage("*total*");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    public void Constructor_Throws_When_Earned_Is_Negative(double earned)
    {
        var act = () => new Score((decimal)earned, 10m);

        act.Should().Throw<DomainException>().WithMessage("*negative*");
    }

    [Theory]
    [InlineData(10.01, 10)]
    [InlineData(11, 10)]
    public void Constructor_Throws_When_Earned_Exceeds_Total(double earned, double total)
    {
        var act = () => new Score((decimal)earned, (decimal)total);

        act.Should().Throw<DomainException>().WithMessage("*exceed*");
    }

    [Fact]
    public void Constructor_Allows_Earned_Equal_To_Total()
    {
        var score = new Score(10m, 10m);

        score.Earned.Should().Be(10m);
        score.Total.Should().Be(10m);
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 100)]
    [InlineData(5, 10, 50)]
    [InlineData(1, 3, 33.33)]
    [InlineData(2, 3, 66.67)]
    [InlineData(7, 8, 87.5)]
    public void Percentage_Is_Rounded_To_Two_Decimals(double earned, double total, double expected)
    {
        var score = new Score((decimal)earned, (decimal)total);

        score.Percentage.Should().Be((decimal)expected);
    }

    [Fact]
    public void Percentage_Uses_AwayFromZero_At_Midpoint()
    {
        // 0.125 / 1.0 = 12.5 % exactly; 12.5 has no midpoint so use a true midpoint case:
        // 1.005 / 100 = 1.005 % rounded to 2dp -> AwayFromZero=1.01, ToEven=1.00
        var score = new Score(1.005m, 100m);

        score.Percentage.Should().Be(1.01m);
    }

    [Fact]
    public void Zero_Factory_Produces_Zero_Earned_With_Given_Total()
    {
        var score = Score.Zero(42m);

        score.Earned.Should().Be(0m);
        score.Total.Should().Be(42m);
        score.Percentage.Should().Be(0m);
    }

    [Fact]
    public void Two_Scores_With_Same_Values_Are_Equal()
    {
        var a = new Score(5m, 10m);
        var b = new Score(5m, 10m);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Scores_With_Different_Values_Are_Not_Equal()
    {
        new Score(5m, 10m).Should().NotBe(new Score(6m, 10m));
        new Score(5m, 10m).Should().NotBe(new Score(5m, 11m));
    }
}
