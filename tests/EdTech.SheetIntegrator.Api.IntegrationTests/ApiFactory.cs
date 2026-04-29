using EdTech.SheetIntegrator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace EdTech.SheetIntegrator.Api.IntegrationTests;

/// <summary>
/// xUnit collection fixture: boots a real SQL Server 2022 container, wires it into a
/// <see cref="WebApplicationFactory{TEntryPoint}"/> for the full ASP.NET Core pipeline,
/// applies EF migrations, then tears everything down on disposal.
///
/// All test classes tagged <c>[Collection(ApiCollection.Name)]</c> share this one instance,
/// so the container starts once per test run.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // JWT settings injected into the test host so we can call /dev/token with known creds.
    internal const string TestIssuer = "edtech-test";
    internal const string TestAudience = "edtech-api-test";
    internal const string TestSigningKey = "integration-test-signing-key-min-32-chars!!";

    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    // ── IAsyncLifetime ────────────────────────────────────────────────────────

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _mssql.StartAsync();

        // Apply migrations through the application's own DbContext so the schema is
        // identical to what production migrations produce.
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mssql.DisposeAsync();
        await base.DisposeAsync();
    }

    // ── WebApplicationFactory ────────────────────────────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use "Development" so the /dev/token endpoint and Scalar UI are mapped.
        builder.UseEnvironment("Development");

        // Supply all required configuration in-memory so the test is self-contained
        // regardless of which appsettings file is discovered at runtime.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _mssql.GetConnectionString(),

                // JWT — must match the token minted by /dev/token
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:TokenLifetimeMinutes"] = "60",

                // Keep Serilog quiet during tests
                ["Serilog:MinimumLevel:Default"] = "Warning",
                ["Serilog:MinimumLevel:Override:Microsoft"] = "Error",
                ["Serilog:MinimumLevel:Override:System"] = "Error",
            });
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Creates an <see cref="HttpClient"/> that does NOT follow redirects.</summary>
    public HttpClient CreateApiClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
