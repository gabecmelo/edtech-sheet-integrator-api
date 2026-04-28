using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;
using EdTech.SheetIntegrator.Domain.UnitTests.TestData;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Assessments;

public class AssessmentTests
{
    public class Construction
    {
        [Fact]
        public void Builds_With_Valid_Inputs()
        {
            var id = Guid.NewGuid();
            var questions = new[] { Fixtures.ExactQuestion(id: "Q1", points: 2m), Fixtures.ExactQuestion(id: "Q2", points: 3m) };

            var a = new Assessment(id, "Quiz", questions, Fixtures.Now);

            a.Id.Should().Be(id);
            a.Title.Should().Be("Quiz");
            a.CreatedAt.Should().Be(Fixtures.Now);
            a.Questions.Should().HaveCount(2);
            a.MaxScore.Should().Be(5m);
        }

        [Fact]
        public void Throws_When_Id_Is_Empty()
        {
            var act = () => new Assessment(Guid.Empty, "Quiz", [Fixtures.ExactQuestion()], Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Assessment id*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Throws_When_Title_Is_Blank(string title)
        {
            var act = () => new Assessment(Guid.NewGuid(), title, [Fixtures.ExactQuestion()], Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*title*");
        }

        [Fact]
        public void Throws_When_Questions_Is_Null()
        {
            var act = () => new Assessment(Guid.NewGuid(), "Quiz", null!, Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Questions*");
        }

        [Fact]
        public void Throws_When_Questions_Is_Empty()
        {
            var act = () => new Assessment(Guid.NewGuid(), "Quiz", [], Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*at least one*");
        }

        [Fact]
        public void Throws_When_Question_Ids_Are_Duplicated()
        {
            var act = () => new Assessment(
                Guid.NewGuid(),
                "Quiz",
                [Fixtures.ExactQuestion(id: "Q1"), Fixtures.ExactQuestion(id: "Q1")],
                Fixtures.Now);

            act.Should().Throw<InvalidQuestionConfigurationException>().WithMessage("*Duplicate*Q1*");
        }
    }

    public class Grading
    {
        private readonly Assessment _assessment = Fixtures.Assessment(
            questions: [
                Fixtures.ExactQuestion(id: "Q1", answer: "Paris", points: 1m),
                Fixtures.CaseInsensitiveQuestion(id: "Q2", answer: "Mitochondria", points: 2m),
                Fixtures.NumericQuestion(id: "Q3", answer: "3.14", points: 5m, tolerance: 0.01m),
            ]);

        [Fact]
        public void Awards_Full_Score_When_All_Answers_Are_Correct()
        {
            var answers = new[]
            {
                new Answer("Q1", "Paris"),
                new Answer("Q2", "MITOCHONDRIA"),
                new Answer("Q3", "3.14"),
            };

            var result = _assessment.Grade(answers, Fixtures.Now);

            result.Score.Earned.Should().Be(8m);
            result.Score.Total.Should().Be(8m);
            result.Score.Percentage.Should().Be(100m);
            result.Outcomes.Should().HaveCount(3);
            result.Outcomes.Should().AllSatisfy(o => o.IsCorrect.Should().BeTrue());
            result.GradedAt.Should().Be(Fixtures.Now);
        }

        [Fact]
        public void Awards_Zero_When_All_Answers_Are_Wrong()
        {
            var answers = new[]
            {
                new Answer("Q1", "London"),
                new Answer("Q2", "nucleus"),
                new Answer("Q3", "0"),
            };

            var result = _assessment.Grade(answers, Fixtures.Now);

            result.Score.Earned.Should().Be(0m);
            result.Score.Percentage.Should().Be(0m);
            result.Outcomes.Should().AllSatisfy(o =>
            {
                o.IsCorrect.Should().BeFalse();
                o.EarnedPoints.Should().Be(0m);
            });
        }

        [Fact]
        public void Awards_Partial_Score_For_Mixed_Correctness()
        {
            var answers = new[]
            {
                new Answer("Q1", "Paris"),    // correct, 1pt
                new Answer("Q2", "wrong"),    // wrong, 0pt
                new Answer("Q3", "3.14"),     // correct, 5pt
            };

            var result = _assessment.Grade(answers, Fixtures.Now);

            result.Score.Earned.Should().Be(6m);
            result.Score.Total.Should().Be(8m);
            result.Score.Percentage.Should().Be(75m);
        }

        [Fact]
        public void Treats_Unanswered_Questions_As_Zero_Points()
        {
            // Only Q1 answered; Q2 and Q3 missing.
            var answers = new[] { new Answer("Q1", "Paris") };

            var result = _assessment.Grade(answers, Fixtures.Now);

            result.Outcomes.Should().HaveCount(3);
            result.Outcomes.Single(o => o.QuestionId == "Q1").IsCorrect.Should().BeTrue();
            result.Outcomes.Single(o => o.QuestionId == "Q2").IsCorrect.Should().BeFalse();
            result.Outcomes.Single(o => o.QuestionId == "Q2").EarnedPoints.Should().Be(0m);
            result.Outcomes.Single(o => o.QuestionId == "Q3").IsCorrect.Should().BeFalse();
            result.Score.Earned.Should().Be(1m);
        }

        [Fact]
        public void Throws_When_Answers_Reference_Unknown_Question_Ids()
        {
            var answers = new[]
            {
                new Answer("Q1", "Paris"),
                new Answer("Q99", "phantom"),
            };

            var act = () => _assessment.Grade(answers, Fixtures.Now);

            act.Should().Throw<MismatchedAnswerSheetException>().WithMessage("*Q99*");
        }

        [Fact]
        public void Throws_When_Answers_Is_Null()
        {
            var act = () => _assessment.Grade(null!, Fixtures.Now);

            act.Should().Throw<MismatchedAnswerSheetException>().WithMessage("*null*");
        }

        [Fact]
        public void Last_Response_Wins_For_Repeated_Question_Ids()
        {
            var answers = new[]
            {
                new Answer("Q1", "London"),  // wrong
                new Answer("Q1", "Paris"),   // correct, last write wins
            };

            var result = _assessment.Grade(answers, Fixtures.Now);

            result.Outcomes.Single(o => o.QuestionId == "Q1").IsCorrect.Should().BeTrue();
            result.Score.Earned.Should().Be(1m);
        }

        [Fact]
        public void Result_Outcome_Order_Follows_Assessment_Question_Order()
        {
            var answers = new[]
            {
                new Answer("Q3", "3.14"),
                new Answer("Q1", "Paris"),
            };

            var result = _assessment.Grade(answers, Fixtures.Now);

            result.Outcomes.Select(o => o.QuestionId).Should().Equal("Q1", "Q2", "Q3");
        }

        [Fact]
        public void Result_GradedAt_Reflects_Caller_Provided_Timestamp()
        {
            var when = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var result = _assessment.Grade([new Answer("Q1", "Paris")], when);

            result.GradedAt.Should().Be(when);
        }
    }

    [Fact]
    public void MaxScore_Is_Sum_Of_Question_Points()
    {
        var a = Fixtures.Assessment(questions: [
            Fixtures.ExactQuestion(id: "Q1", points: 1.5m),
            Fixtures.ExactQuestion(id: "Q2", points: 2.5m),
            Fixtures.ExactQuestion(id: "Q3", points: 4m),
        ]);

        a.MaxScore.Should().Be(8m);
    }

    [Fact]
    public void DomainEvents_Is_Initially_Empty()
    {
        var a = Fixtures.Assessment();

        a.DomainEvents.Should().BeEmpty();
    }
}
