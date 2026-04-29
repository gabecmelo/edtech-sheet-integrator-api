using System.Net;
using EdTech.SheetIntegrator.Api.IntegrationTests;

namespace EdTech.SheetIntegrator.Api.IntegrationTests.Health;

[Collection(ApiCollection.Name)]
public sealed class HealthTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateApiClient();

    [Fact]
    public async Task Live_Returns_200()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_Returns_200_When_Database_Reachable()
    {
        // The container is running and migrations applied by ApiFactory.InitializeAsync.
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
