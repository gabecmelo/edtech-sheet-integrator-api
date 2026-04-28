using EdTech.SheetIntegrator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace EdTech.SheetIntegrator.Infrastructure.IntegrationTests.Persistence;

/// <summary>
/// xUnit collection fixture: spins up a single SQL Server 2022 container for the entire
/// test class collection, applies EF migrations, and tears it down on disposal. Each test
/// builds its own <see cref="AppDbContext"/> against the shared connection string and
/// uses unique <see cref="Guid"/> ids to stay isolated from sibling tests.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var ctx = CreateContext();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new AppDbContext(options);
    }
}

[CollectionDefinition(Name)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "xUnit collection-fixture convention: marker classes are named *Collection.")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}
