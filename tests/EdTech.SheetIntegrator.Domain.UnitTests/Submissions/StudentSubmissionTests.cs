using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;
using EdTech.SheetIntegrator.Domain.UnitTests.TestData;

namespace EdTech.SheetIntegrator.Domain.UnitTests.Submissions;

public class StudentSubmissionTests
{
    public class Construction
    {
        [Fact]
        public void Builds_With_Valid_Inputs()
        {
            var id = Guid.NewGuid();
            var assessmentId = Guid.NewGuid();
            var answers = new[] { new Answer("Q1", "Paris") };

            var s = new StudentSubmission(id, assessmentId, "alice@x.com", answers, "alice.xlsx", Fixtures.Now);

            s.Id.Should().Be(id);
            s.AssessmentId.Should().Be(assessmentId);
            s.StudentIdentifier.Should().Be("alice@x.com");
            s.Answers.Should().HaveCount(1);
            s.SourceFileName.Should().Be("alice.xlsx");
            s.SubmittedAt.Should().Be(Fixtures.Now);
            s.Result.Should().BeNull();
            s.IsGraded.Should().BeFalse();
        }

        [Fact]
        public void Throws_When_Id_Is_Empty()
        {
            var act = () => new StudentSubmission(
                Guid.Empty, Guid.NewGuid(), "alice", [new Answer("Q1", "x")], "f.xlsx", Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Submission id*");
        }

        [Fact]
        public void Throws_When_AssessmentId_Is_Empty()
        {
            var act = () => new StudentSubmission(
                Guid.NewGuid(), Guid.Empty, "alice", [new Answer("Q1", "x")], "f.xlsx", Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Assessment id*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Throws_When_StudentIdentifier_Is_Blank(string identifier)
        {
            var act = () => new StudentSubmission(
                Guid.NewGuid(), Guid.NewGuid(), identifier, [new Answer("Q1", "x")], "f.xlsx", Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Student identifier*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Throws_When_SourceFileName_Is_Blank(string name)
        {
            var act = () => new StudentSubmission(
                Guid.NewGuid(), Guid.NewGuid(), "alice", [new Answer("Q1", "x")], name, Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Source file name*");
        }

        [Fact]
        public void Throws_When_Answers_Is_Null()
        {
            var act = () => new StudentSubmission(
                Guid.NewGuid(), Guid.NewGuid(), "alice", null!, "f.xlsx", Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*Answers*null*");
        }

        [Fact]
        public void Throws_When_Answers_Is_Empty()
        {
            var act = () => new StudentSubmission(
                Guid.NewGuid(), Guid.NewGuid(), "alice", [], "f.xlsx", Fixtures.Now);

            act.Should().Throw<DomainException>().WithMessage("*at least one*");
        }
    }

    public class GradingLifecycle
    {
        private static GradingResult ValidResult(decimal earned = 1m, decimal total = 1m) =>
            new(new Score(earned, total), [new QuestionOutcome("Q1", earned == total, earned, total)], Fixtures.Now);

        [Fact]
        public void AttachResult_Sets_Result_And_Marks_As_Graded()
        {
            var s = Fixtures.Submission();
            var result = ValidResult();

            s.AttachResult(result);

            s.Result.Should().BeSameAs(result);
            s.IsGraded.Should().BeTrue();
        }

        [Fact]
        public void AttachResult_Raises_SubmissionGraded_With_Correct_Payload()
        {
            var s = Fixtures.Submission();
            var result = ValidResult(earned: 1m, total: 1m);

            s.AttachResult(result);

            s.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<SubmissionGraded>()
                .Which.Should().Match<SubmissionGraded>(e =>
                    e.SubmissionId == s.Id &&
                    e.AssessmentId == s.AssessmentId &&
                    e.Score == result.Score &&
                    e.OccurredAt == result.GradedAt);
        }

        [Fact]
        public void AttachResult_Throws_When_Submission_Already_Graded()
        {
            var s = Fixtures.Submission();
            s.AttachResult(ValidResult());

            var act = () => s.AttachResult(ValidResult());

            act.Should().Throw<DomainException>().WithMessage("*already*graded*");
        }

        [Fact]
        public void ClearDomainEvents_Empties_The_Collection()
        {
            var s = Fixtures.Submission();
            s.AttachResult(ValidResult());
            s.DomainEvents.Should().NotBeEmpty();

            s.ClearDomainEvents();

            s.DomainEvents.Should().BeEmpty();
        }
    }
}
