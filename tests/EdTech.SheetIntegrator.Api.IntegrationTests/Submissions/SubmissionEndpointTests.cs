using System.Net;
using System.Text;
using EdTech.SheetIntegrator.Api.IntegrationTests.Helpers;

namespace EdTech.SheetIntegrator.Api.IntegrationTests.Submissions;

/// <summary>
/// End-to-end tests covering the full submission and grading pipeline:
/// file upload → parse → grade → persist → retrieve.
/// Each test is independent: it creates its own assessment so data never leaks between tests.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class SubmissionEndpointTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateApiClient();

    // Answers that match the standard two-question key (Q1=Paris exact, Q2=3.14±0.01 numeric).
    private static readonly (string, string)[] _allCorrect = [("Q1", "Paris"), ("Q2", "3.14")];
    private static readonly (string, string)[] _onlyQ1Correct = [("Q1", "Paris"), ("Q2", "wrong")];

    // ── Authorization ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitSheet_Returns_401_Without_Bearer_Token()
    {
        using var form = ApiRequestBuilder.BuildSubmissionForm(
            "alice@example.com",
            FileFixtures.AsCsvContent(FileFixtures.BuildCsv(_allCorrect)));

        var response = await _client.PostAsync(
            $"/api/v1/assessments/{Guid.NewGuid()}/submissions", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path — CSV ──────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitCsv_Returns_201_With_FullyGraded_Result()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var assessmentId = await ApiRequestBuilder.CreateAssessmentAsync(_client);

        using var form = ApiRequestBuilder.BuildSubmissionForm(
            "alice@example.com",
            FileFixtures.AsCsvContent(FileFixtures.BuildCsv(_allCorrect)));

        var response = await _client.PostAsync(
            $"/api/v1/assessments/{assessmentId}/submissions", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await ApiRequestBuilder.ReadAsync<ApiRequestBuilder.SubmissionBody>(response);
        body.Should().NotBeNull();
        body!.IsGraded.Should().BeTrue();
        body.Earned.Should().Be(6m, "Q1=1 + Q2=5");
        body.Total.Should().Be(6m);
        body.Percentage.Should().Be(100m);
    }

    [Fact]
    public async Task SubmitCsv_Partial_Score_When_One_Answer_Wrong()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var assessmentId = await ApiRequestBuilder.CreateAssessmentAsync(_client);

        using var form = ApiRequestBuilder.BuildSubmissionForm(
            "bob@example.com",
            FileFixtures.AsCsvContent(FileFixtures.BuildCsv(_onlyQ1Correct)));

        var response = await _client.PostAsync(
            $"/api/v1/assessments/{assessmentId}/submissions", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await ApiRequestBuilder.ReadAsync<ApiRequestBuilder.SubmissionBody>(response);
        body!.Earned.Should().Be(1m, "only Q1 is correct");
        body.Total.Should().Be(6m);
        body.Percentage.Should().Be(17m, "round(1/6*100) = 17");
    }

    // ── Happy path — XLSX ─────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitXlsx_Returns_201_And_GradedResult()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var assessmentId = await ApiRequestBuilder.CreateAssessmentAsync(_client);

        using var form = ApiRequestBuilder.BuildSubmissionForm(
            "charlie@example.com",
            FileFixtures.AsXlsxContent(FileFixtures.BuildXlsx(_allCorrect)));

        var response = await _client.PostAsync(
            $"/api/v1/assessments/{assessmentId}/submissions", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await ApiRequestBuilder.ReadAsync<ApiRequestBuilder.SubmissionBody>(response);
        body!.IsGraded.Should().BeTrue();
        body.Earned.Should().Be(6m);
    }

    // ── Retrieve submission ───────────────────────────────────────────────────

    [Fact]
    public async Task GetSubmission_Returns_200_With_Graded_Data()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var assessmentId = await ApiRequestBuilder.CreateAssessmentAsync(_client);
        var submissionId = await ApiRequestBuilder.SubmitCsvAsync(_client, assessmentId, _allCorrect);

        // GET is an open endpoint — strip auth header.
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/api/v1/submissions/{submissionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ApiRequestBuilder.ReadAsync<ApiRequestBuilder.SubmissionBody>(response);
        body!.Id.Should().Be(submissionId);
        body.IsGraded.Should().BeTrue();
        body.Earned.Should().Be(6m);
    }

    [Fact]
    public async Task GetSubmission_Returns_404_For_Unknown_Id()
    {
        var response = await _client.GetAsync($"/api/v1/submissions/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var code = await ApiRequestBuilder.ReadProblemCodeAsync(response);
        code.Should().Be("submission.not_found");
    }

    // ── List submissions ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListSubmissions_Returns_Paged_Results_Newest_First()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var assessmentId = await ApiRequestBuilder.CreateAssessmentAsync(_client);

        // Submit two different students.
        await ApiRequestBuilder.SubmitCsvAsync(_client, assessmentId, _allCorrect, "student-a@x.com");
        await ApiRequestBuilder.SubmitCsvAsync(_client, assessmentId, _onlyQ1Correct, "student-b@x.com");

        var response = await _client.GetAsync(
            $"/api/v1/assessments/{assessmentId}/submissions?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = doc.RootElement;
        root.GetProperty("totalCount").GetInt32().Should().Be(2);
        root.GetProperty("page").GetInt32().Should().Be(1);
        root.GetProperty("hasNextPage").GetBoolean().Should().BeFalse();
        root.GetProperty("items").GetArrayLength().Should().Be(2);
    }

    // ── Error paths ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitSheet_Returns_404_For_Unknown_Assessment()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        using var form = ApiRequestBuilder.BuildSubmissionForm(
            "alice@example.com",
            FileFixtures.AsCsvContent(FileFixtures.BuildCsv(_allCorrect)));

        var response = await _client.PostAsync(
            $"/api/v1/assessments/{Guid.Empty}/submissions", form);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var code = await ApiRequestBuilder.ReadProblemCodeAsync(response);
        code.Should().Be("assessment.not_found");
    }

    [Fact]
    public async Task SubmitSheet_Returns_415_For_Unsupported_File_Type()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var assessmentId = await ApiRequestBuilder.CreateAssessmentAsync(_client);

        using var form = ApiRequestBuilder.BuildSubmissionForm(
            "alice@example.com",
            FileFixtures.AsRawContent(
                Encoding.UTF8.GetBytes("not a spreadsheet"),
                "answers.txt",
                "text/plain"));

        var response = await _client.PostAsync(
            $"/api/v1/assessments/{assessmentId}/submissions", form);

        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        var code = await ApiRequestBuilder.ReadProblemCodeAsync(response);
        code.Should().Be("submission.unsupported_file_type");
    }
}
