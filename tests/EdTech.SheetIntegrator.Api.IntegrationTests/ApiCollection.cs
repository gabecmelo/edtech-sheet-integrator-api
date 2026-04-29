namespace EdTech.SheetIntegrator.Api.IntegrationTests;

/// <summary>
/// Binds all API integration-test classes to a single shared <see cref="ApiFactory"/>
/// instance. xUnit runs the collection sequentially, so tests do not race against each
/// other or the shared SQL Server container.
/// </summary>
[CollectionDefinition(Name)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "xUnit collection-fixture convention: marker classes are named *Collection.")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
