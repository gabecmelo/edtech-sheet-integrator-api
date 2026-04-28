using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Assessments;

public class QuestionTests
{
    public class Construction
    {
        [Fact]
        public void Builds_An_Exact_Question_With_Valid_Inputs()
        {
            var q = new Question("Q1", "Capital of France?", "Paris", 1m, MatchMode.Exact);

            q.QuestionId.Should().Be("Q1");
            q.Prompt.Should().Be("Capital of France?");
            q.CorrectAnswer.Should().Be("Paris");
            q.Points.Should().Be(1m);
            q.MatchMode.Should().Be(MatchMode.Exact);
            q.NumericTolerance.Should().BeNull();
        }

        [Fact]
        public void Builds_A_Numeric_Question_With_Tolerance()
        {
            var q = new Question("Q1", "Pi?", "3.14", 5m, MatchMode.Numeric, numericTolerance: 0.01m);

            q.MatchMode.Should().Be(MatchMode.Numeric);
            q.NumericTolerance.Should().Be(0.01m);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Throws_When_QuestionId_Is_Blank(string id)
        {
            var act = () => new Question(id, "p", "a", 1m, MatchMode.Exact);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*Question id*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Throws_When_Prompt_Is_Blank(string prompt)
        {
            var act = () => new Question("Q1", prompt, "a", 1m, MatchMode.Exact);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*Prompt*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-0.01)]
        public void Throws_When_Points_Are_Not_Positive(double points)
        {
            var act = () => new Question("Q1", "p", "a", (decimal)points, MatchMode.Exact);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*Points*");
        }

        [Fact]
        public void Throws_When_Numeric_Mode_Has_No_Tolerance()
        {
            var act = () => new Question("Q1", "p", "1", 1m, MatchMode.Numeric, numericTolerance: null);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*tolerance*non-negative*");
        }

        [Fact]
        public void Throws_When_Numeric_Mode_Has_Negative_Tolerance()
        {
            var act = () => new Question("Q1", "p", "1", 1m, MatchMode.Numeric, numericTolerance: -0.01m);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*tolerance*non-negative*");
        }

        [Fact]
        public void Allows_Numeric_Mode_With_Zero_Tolerance()
        {
            var q = new Question("Q1", "p", "1", 1m, MatchMode.Numeric, numericTolerance: 0m);

            q.NumericTolerance.Should().Be(0m);
        }

        [Fact]
        public void Throws_When_Numeric_Mode_Has_NonNumeric_CorrectAnswer()
        {
            var act = () => new Question("Q1", "p", "not-a-number", 1m, MatchMode.Numeric, 0.5m);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*number*");
        }

        [Fact]
        public void Throws_When_Tolerance_Is_Set_For_NonNumeric_Mode()
        {
            var act = () => new Question("Q1", "p", "Paris", 1m, MatchMode.Exact, numericTolerance: 0.5m);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*Numeric*");
        }

        [Fact]
        public void Throws_When_Match_Mode_Is_Undefined()
        {
            var act = () => new Question("Q1", "p", "a", 1m, (MatchMode)999);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*match mode*");
        }
    }

    public class ExactMatching
    {
        private readonly Question _q = new("Q1", "p", "Paris", 1m, MatchMode.Exact);

        [Fact]
        public void Matches_Identical_Response() => _q.Matches("Paris").Should().BeTrue();

        [Fact]
        public void Does_Not_Match_Case_Difference() => _q.Matches("paris").Should().BeFalse();

        [Fact]
        public void Does_Not_Match_Whitespace_Padding() => _q.Matches(" Paris ").Should().BeFalse();

        [Fact]
        public void Does_Not_Match_Different_Word() => _q.Matches("London").Should().BeFalse();

        [Fact]
        public void Does_Not_Match_Empty() => _q.Matches(string.Empty).Should().BeFalse();

        [Fact]
        public void Does_Not_Match_Null() => _q.Matches(null).Should().BeFalse();
    }

    public class CaseInsensitiveMatching
    {
        private readonly Question _q = new("Q1", "p", "Mitochondria", 1m, MatchMode.CaseInsensitive);

        [Theory]
        [InlineData("Mitochondria")]
        [InlineData("mitochondria")]
        [InlineData("MITOCHONDRIA")]
        [InlineData("  mitochondria  ")]
        [InlineData("\tMITOCHONDRIA\n")]
        public void Matches_Case_And_Whitespace_Variants(string response) =>
            _q.Matches(response).Should().BeTrue();

        [Theory]
        [InlineData("nucleus")]
        [InlineData("Mito chondria")]
        [InlineData("")]
        [InlineData(null)]
        public void Does_Not_Match_Different_Content(string? response) =>
            _q.Matches(response).Should().BeFalse();
    }

    public class NumericMatching
    {
        private readonly Question _q = new("Q1", "p", "3.14", 1m, MatchMode.Numeric, numericTolerance: 0.01m);

        [Theory]
        [InlineData("3.14")]
        [InlineData("3.13")]
        [InlineData("3.15")]
        [InlineData("3.140")]
        public void Matches_Within_Tolerance(string response) =>
            _q.Matches(response).Should().BeTrue();

        [Theory]
        [InlineData("3.16")]
        [InlineData("3.12")]
        [InlineData("3")]
        public void Does_Not_Match_Outside_Tolerance(string response) =>
            _q.Matches(response).Should().BeFalse();

        [Theory]
        [InlineData("not-a-number")]
        [InlineData("")]
        [InlineData(null)]
        public void Does_Not_Match_Non_Numeric_Response(string? response) =>
            _q.Matches(response).Should().BeFalse();

        [Fact]
        public void Numeric_Comparison_Uses_Invariant_Culture_For_Decimal_Separator()
        {
            // German locale would use "3,14"; we always parse with InvariantCulture so "3,14" must NOT match.
            _q.Matches("3,14").Should().BeFalse();
        }

        [Fact]
        public void Zero_Tolerance_Requires_Exact_Numeric_Equality()
        {
            var strict = new Question("Q1", "p", "10", 1m, MatchMode.Numeric, numericTolerance: 0m);

            strict.Matches("10").Should().BeTrue();
            strict.Matches("10.0001").Should().BeFalse();
        }

        [Fact]
        public void Numeric_Match_Is_Symmetric_Around_Expected()
        {
            // Tolerance 0.5 around expected 100 -> 99.5 and 100.5 both match.
            var q = new Question("Q1", "p", "100", 1m, MatchMode.Numeric, numericTolerance: 0.5m);

            q.Matches("99.5").Should().BeTrue();
            q.Matches("100.5").Should().BeTrue();
            q.Matches("99.49").Should().BeFalse();
            q.Matches("100.51").Should().BeFalse();
        }
    }

    [Fact]
    public void Two_Questions_With_Same_Values_Are_Equal_By_Record_Equality()
    {
        var a = new Question("Q1", "p", "Paris", 1m, MatchMode.Exact);
        var b = new Question("Q1", "p", "Paris", 1m, MatchMode.Exact);

        a.Should().Be(b);
    }
}
