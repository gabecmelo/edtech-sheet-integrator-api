using System.Net.Http.Json;

namespace EdTech.SheetIntegrator.Api.IntegrationTests.Helpers;

/// <summary>
/// Mints a dev JWT by calling <c>POST /dev/token</c> (Development-env-only endpoint)
/// and returns the bearer token string for use in subsequent requests.
/// </summary>
internal static class AuthHelper
{
    public static async Task<string> GetInstructorTokenAsync(HttpClient client, string subject = "test-instructor@local")
    {
        var response = await client.PostAsJsonAsync("/dev/token", new { subject });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new InvalidOperationException("Empty /dev/token response.");

        return body.Token;
    }

    public static void UseInstructorToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private sealed record TokenResponse(string Token, string Subject);
}
