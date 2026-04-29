using System.Net;
using System.Net.Http.Json;
using EdTech.SheetIntegrator.Api.IntegrationTests.Helpers;

namespace EdTech.SheetIntegrator.Api.IntegrationTests.Assessments;

/// <summary>
/// End-to-end tests for POST /api/v1/assessments and GET /api/v1/assessments/{id}.
/// Each test creates its own assessment so tests are fully independent.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class AssessmentEndpointTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateApiClient();

    // ── Authorization ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAssessment_Returns_401_Without_Bearer_Token()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/assessments",
            ApiRequestBuilder.StandardAssessmentBody());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAssessment_Returns_201_With_Location_And_Correct_Body()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/assessments",
            ApiRequestBuilder.StandardAssessmentBody(),
            ApiRequestBuilder.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull("a Location header must point to the created resource");

        var body = await ApiRequestBuilder.ReadAsync<ApiRequestBuilder.AssessmentBody>(response);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Title.Should().Be("Geography & Math Quiz");
        body.MaxScore.Should().Be(6m, "Q1=1pt + Q2=5pt");
    }

    [Fact]
    public async Task GetAssessment_Returns_200_With_Same_Data_As_Create()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        // Create first, then read back via open GET endpoint.
        var id = await ApiRequestBuilder.CreateAssessmentAsync(_client);

        // GET is an open endpoint — remove auth header to prove it.
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/api/v1/assessments/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ApiRequestBuilder.ReadAsync<ApiRequestBuilder.AssessmentBody>(response);
        body!.Id.Should().Be(id);
        body.MaxScore.Should().Be(6m);
    }

    // ── Validation errors ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAssessment_Returns_400_When_Title_Is_Empty()
    {
        var token = await AuthHelper.GetInstructorTokenAsync(_client);
        _client.UseInstructorToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/assessments", new
        {
            title = "",
            questions = Array.Empty<object>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var code = await ApiRequestBuilder.ReadProblemCodeAsync(response);
        code.Should().Be("validation.failed");
    }

    // ── Not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAssessment_Returns_404_For_Unknown_Id()
    {
        var response = await _client.GetAsync(
            $"/api/v1/assessments/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var code = await ApiRequestBuilder.ReadProblemCodeAsync(response);
        code.Should().Be("assessment.not_found");
    }
}
