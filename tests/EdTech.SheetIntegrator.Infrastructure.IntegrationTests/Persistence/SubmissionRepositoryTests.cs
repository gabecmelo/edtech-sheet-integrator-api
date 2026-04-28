using EdTech.SheetIntegrator.Infrastructure.IntegrationTests.TestData;
using EdTech.SheetIntegrator.Infrastructure.Persistence.Repositories;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.Persistence;

[Collection(DatabaseCollection.Name)]
public class SubmissionRepositoryTests
{
    private readonly DatabaseFixture _db;

    public SubmissionRepositoryTests(DatabaseFixture db)
    {
        _db = db;
    }

    private async Task SeedAssessmentAsync(EdTech.SheetIntegrator.Domain.Assessments.Assessment assessment)
    {
        await using var ctx = _db.CreateContext();
        await new AssessmentRepository(ctx).AddAsync(assessment, CancellationToken.None);
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task Ungraded_Submission_Persists_With_Null_ResultJson()
    {
        var assessment = IntegrationFixtures.AssessmentWithTwoQuestions();
        await SeedAssessmentAsync(assessment);

        var submission = IntegrationFixtures.UngradedSubmission(assessment.Id);

        await using (var writeCtx = _db.CreateContext())
        {
            await new SubmissionRepository(writeCtx).AddAsync(submission, CancellationToken.None);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = _db.CreateContext();
        var loaded = await new SubmissionRepository(readCtx).GetByIdAsync(submission.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.IsGraded.Should().BeFalse();
        loaded.Result.Should().BeNull();
        loaded.Answers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Graded_Submission_Roundtrips_Score_And_Outcomes()
    {
        var assessment = IntegrationFixtures.AssessmentWithTwoQuestions();
        await SeedAssessmentAsync(assessment);

        var submission = IntegrationFixtures.GradedSubmission(assessment);

        await using (var writeCtx = _db.CreateContext())
        {
            await new SubmissionRepository(writeCtx).AddAsync(submission, CancellationToken.None);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = _db.CreateContext();
        var loaded = await new SubmissionRepository(readCtx).GetByIdAsync(submission.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.IsGraded.Should().BeTrue();
        loaded.Result.Should().NotBeNull();
        loaded.Result!.Score.Earned.Should().Be(6m);
        loaded.Result.Score.Total.Should().Be(6m);
        loaded.Result.Score.Percentage.Should().Be(100m);
        loaded.Result.Outcomes.Should().HaveCount(2);
        loaded.Result.Outcomes.Should().OnlyContain(o => o.IsCorrect);
        loaded.Result.GradedAt.Should().Be(IntegrationFixtures.Now.AddMinutes(1));
    }

    [Fact]
    public async Task GetById_Returns_Null_When_Missing()
    {
        await using var ctx = _db.CreateContext();
        var loaded = await new SubmissionRepository(ctx)
            .GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task ListByAssessment_Pages_And_Orders_By_SubmittedAt_Descending()
    {
        var assessment = IntegrationFixtures.AssessmentWithTwoQuestions();
        await SeedAssessmentAsync(assessment);

        // Seed 5 submissions with explicit timestamps (oldest -> newest by index).
        var submitted = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            await using var seedCtx = _db.CreateContext();
            var sub = new EdTech.SheetIntegrator.Domain.Submissions.StudentSubmission(
                Guid.NewGuid(),
                assessment.Id,
                $"student-{i}@x.com",
                [new EdTech.SheetIntegrator.Domain.Submissions.Answer("Q1", "Paris")],
                $"sheet-{i}.xlsx",
                IntegrationFixtures.Now.AddMinutes(i));
            await new SubmissionRepository(seedCtx).AddAsync(sub, CancellationToken.None);
            await seedCtx.SaveChangesAsync();
            submitted.Add(sub.Id);
        }

        await using var readCtx = _db.CreateContext();
        var repo = new SubmissionRepository(readCtx);

        var page1 = await repo.ListByAssessmentAsync(assessment.Id, skip: 0, take: 2, CancellationToken.None);
        var page2 = await repo.ListByAssessmentAsync(assessment.Id, skip: 2, take: 2, CancellationToken.None);
        var count = await repo.CountByAssessmentAsync(assessment.Id, CancellationToken.None);

        count.Should().Be(5);
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);

        // Newest first: indices 4, 3 on page 1; 2, 1 on page 2
        page1[0].StudentIdentifier.Should().Be("student-4@x.com");
        page1[1].StudentIdentifier.Should().Be("student-3@x.com");
        page2[0].StudentIdentifier.Should().Be("student-2@x.com");
        page2[1].StudentIdentifier.Should().Be("student-1@x.com");
    }

    [Fact]
    public async Task CountByAssessment_Is_Scoped_To_The_Assessment()
    {
        var assessmentA = IntegrationFixtures.AssessmentWithTwoQuestions();
        var assessmentB = IntegrationFixtures.AssessmentWithTwoQuestions();
        await SeedAssessmentAsync(assessmentA);
        await SeedAssessmentAsync(assessmentB);

        await using (var ctx = _db.CreateContext())
        {
            await new SubmissionRepository(ctx).AddAsync(
                IntegrationFixtures.UngradedSubmission(assessmentA.Id, "a@x.com"), CancellationToken.None);
            await new SubmissionRepository(ctx).AddAsync(
                IntegrationFixtures.UngradedSubmission(assessmentB.Id, "b@x.com"), CancellationToken.None);
            await new SubmissionRepository(ctx).AddAsync(
                IntegrationFixtures.UngradedSubmission(assessmentB.Id, "c@x.com"), CancellationToken.None);
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = _db.CreateContext();
        var repo = new SubmissionRepository(readCtx);

        (await repo.CountByAssessmentAsync(assessmentA.Id, CancellationToken.None)).Should().Be(1);
        (await repo.CountByAssessmentAsync(assessmentB.Id, CancellationToken.None)).Should().Be(2);
    }
}
