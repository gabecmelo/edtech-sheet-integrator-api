using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Infrastructure.IntegrationTests.TestData;
using EdTech.SheetIntegrator.Infrastructure.Persistence.Repositories;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.Persistence;

[Collection(DatabaseCollection.Name)]
public class AssessmentRepositoryTests
{
    private readonly DatabaseFixture _db;

    public AssessmentRepositoryTests(DatabaseFixture db)
    {
        _db = db;
    }

    [Fact]
    public async Task Add_And_GetById_Round_Trips_Assessment_With_Questions_Json()
    {
        var assessment = IntegrationFixtures.AssessmentWithTwoQuestions();

        await using (var writeCtx = _db.CreateContext())
        {
            var repo = new AssessmentRepository(writeCtx);
            await repo.AddAsync(assessment, CancellationToken.None);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = _db.CreateContext();
        var loaded = await new AssessmentRepository(readCtx).GetByIdAsync(assessment.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(assessment.Id);
        loaded.Title.Should().Be("Sample quiz");
        loaded.Questions.Should().HaveCount(2);
        loaded.Questions.Should().ContainSingle(q => q.QuestionId == "Q1" && q.MatchMode == MatchMode.Exact);
        loaded.Questions.Should().ContainSingle(q => q.QuestionId == "Q2"
            && q.MatchMode == MatchMode.Numeric
            && q.NumericTolerance == 0.01m);
        loaded.MaxScore.Should().Be(6m);
    }

    [Fact]
    public async Task GetById_Returns_Null_When_Missing()
    {
        await using var ctx = _db.CreateContext();
        var repo = new AssessmentRepository(ctx);

        var loaded = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task Persisted_Question_Roundtrips_Match_Behavior()
    {
        // Persisting and reloading a Question must preserve enough state that grading still works
        // identically. This protects against silent breakage where the JSON column drops a field.
        var assessment = IntegrationFixtures.AssessmentWithTwoQuestions();

        await using (var writeCtx = _db.CreateContext())
        {
            await new AssessmentRepository(writeCtx).AddAsync(assessment, CancellationToken.None);
            await writeCtx.SaveChangesAsync();
        }

        await using var readCtx = _db.CreateContext();
        var loaded = await new AssessmentRepository(readCtx).GetByIdAsync(assessment.Id, CancellationToken.None);

        var q1 = loaded!.Questions.Single(q => q.QuestionId == "Q1");
        q1.Matches("Paris").Should().BeTrue();
        q1.Matches("paris").Should().BeFalse(); // Exact mode preserved

        var q2 = loaded.Questions.Single(q => q.QuestionId == "Q2");
        q2.Matches("3.14").Should().BeTrue();
        q2.Matches("3.13").Should().BeTrue();   // within tolerance
        q2.Matches("3.12").Should().BeFalse();  // outside tolerance
    }
}
