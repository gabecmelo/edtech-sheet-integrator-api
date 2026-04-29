using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdTech.SheetIntegrator.Api.IntegrationTests.Helpers;

/// <summary>Shared JSON options + convenience wrappers for reading problem-details responses.</summary>
internal static class ApiRequestBuilder
{
    // Matches ASP.NET Core Minimal API defaults (camelCase, case-insensitive).
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task<T?> ReadAsync<T>(HttpResponseMessage response)
        => response.Content.ReadFromJsonAsync<T>(JsonOptions);

    /// <summary>
    /// Reads the <c>code</c> extension from a RFC 7807 ProblemDetails response body.
    /// The code is serialized as a top-level JSON property by <see cref="ErrorMapping.ResultExtensions"/>.
    /// </summary>
    public static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response)
    {
        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        return doc.RootElement.TryGetProperty("code", out var codeEl)
            ? codeEl.GetString()
            : null;
    }

    // ── Assessment body helpers ───────────────────────────────────────────────

    /// <summary>Standard two-question answer key used across happy-path tests.</summary>
    public static object StandardAssessmentBody() => new
    {
        title = "Geography & Math Quiz",
        questions = new[]
        {
            new
            {
                questionId = "Q1",
                prompt = "Capital of France?",
                correctAnswer = "Paris",
                points = 1,
                matchMode = 0,            // MatchMode.Exact
                numericTolerance = (decimal?)null,
            },
            new
            {
                questionId = "Q2",
                prompt = "Pi to two decimals?",
                correctAnswer = "3.14",
                points = 5,
                matchMode = 2,            // MatchMode.Numeric
                numericTolerance = (decimal?)0.01m,
            },
        },
    };

    /// <summary>Creates an assessment and returns its ID, asserting 201.</summary>
    public static async Task<Guid> CreateAssessmentAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/assessments", StandardAssessmentBody(), JsonOptions);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var body = await ReadAsync<AssessmentBody>(response);
        return body!.Id;
    }

    /// <summary>Submits a CSV file and returns the submission ID, asserting 201.</summary>
    public static async Task<Guid> SubmitCsvAsync(HttpClient client, Guid assessmentId,
        (string QuestionId, string Response)[] answers, string student = "alice@example.com")
    {
        using var form = BuildSubmissionForm(
            student,
            FileFixtures.AsCsvContent(FileFixtures.BuildCsv(answers)));

        var response = await client.PostAsync(
            $"/api/v1/assessments/{assessmentId}/submissions", form);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var body = await ReadAsync<SubmissionBody>(response);
        return body!.Id;
    }

    public static MultipartFormDataContent BuildSubmissionForm(
        string studentIdentifier, HttpContent fileContent)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(studentIdentifier), "studentIdentifier");
        form.Add(fileContent, "file");
        return form;
    }

    // Minimal deserialization targets (only the fields the tests assert on).
    internal sealed record AssessmentBody(Guid Id, string Title, decimal MaxScore);
    internal sealed record SubmissionBody(Guid Id, bool IsGraded, decimal? Earned, decimal? Total, decimal? Percentage);
}
