using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Submissions;

public class AnswerTests
{
    [Fact]
    public void Constructor_Sets_Properties()
    {
        var a = new Answer("Q1", "Paris");

        a.QuestionId.Should().Be("Q1");
        a.Response.Should().Be("Paris");
    }

    [Fact]
    public void Constructor_Allows_Empty_Response()
    {
        var a = new Answer("Q1", string.Empty);

        a.Response.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Coerces_Null_Response_To_Empty()
    {
        var a = new Answer("Q1", null!);

        a.Response.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_When_QuestionId_Is_Blank(string? questionId)
    {
        var act = () => new Answer(questionId!, "x");

        act.Should().Throw<DomainException>().WithMessage("*question id*");
    }

    [Fact]
    public void Two_Answers_With_Same_Values_Are_Equal()
    {
        new Answer("Q1", "Paris").Should().Be(new Answer("Q1", "Paris"));
    }
}
