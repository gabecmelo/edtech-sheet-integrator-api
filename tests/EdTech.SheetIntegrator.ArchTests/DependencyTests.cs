using System.Reflection;
using EdTech.SheetIntegrator.Application.DependencyInjection;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Infrastructure.DependencyInjection;
using NetArchTest.Rules;

namespace EdTech.SheetIntegrator.ArchTests;

/// <summary>
/// Enforces the Clean Architecture dependency rule:
/// each layer may only reference layers beneath it in the hierarchy.
///
///   Api → Infrastructure → Application → Domain
///                                  ↑           ↑
///                         (may also reference Domain directly)
///
/// Any violation of these rules indicates that a refactor has broken the
/// architectural contract — the production code, not the test, must be fixed.
/// </summary>
public sealed class DependencyTests
{
    // ── Assembly anchors ──────────────────────────────────────────────────────
    // One public type from each layer is enough to resolve the assembly at test-time.

    private static readonly Assembly _domainAssembly =
        typeof(Assessment).Assembly;                                 // Domain

    private static readonly Assembly _applicationAssembly =
        typeof(IApplicationAssemblyMarker).Assembly;                 // Application

    private static readonly Assembly _infrastructureAssembly =
        typeof(InfrastructureServiceCollectionExtensions).Assembly;  // Infrastructure

    private static readonly Assembly _apiAssembly =
        typeof(Program).Assembly;                                    // Api

    // ── Namespace prefix constants ────────────────────────────────────────────

    private const string _domainNs = "EdTech.SheetIntegrator.Domain";
    private const string _applicationNs = "EdTech.SheetIntegrator.Application";
    private const string _infrastructureNs = "EdTech.SheetIntegrator.Infrastructure";
    private const string _apiNs = "EdTech.SheetIntegrator.Api";

    // ── Domain — depends on nothing ───────────────────────────────────────────

    [Fact]
    public void Domain_Does_Not_Depend_On_Application()
    {
        AssertNoDependency(_domainAssembly, _applicationNs);
    }

    [Fact]
    public void Domain_Does_Not_Depend_On_Infrastructure()
    {
        AssertNoDependency(_domainAssembly, _infrastructureNs);
    }

    [Fact]
    public void Domain_Does_Not_Depend_On_Api()
    {
        AssertNoDependency(_domainAssembly, _apiNs);
    }

    // ── Application — depends on Domain only ──────────────────────────────────

    [Fact]
    public void Application_Does_Not_Depend_On_Infrastructure()
    {
        AssertNoDependency(_applicationAssembly, _infrastructureNs);
    }

    [Fact]
    public void Application_Does_Not_Depend_On_Api()
    {
        AssertNoDependency(_applicationAssembly, _apiNs);
    }

    // ── Infrastructure — depends on Application + Domain ─────────────────────

    [Fact]
    public void Infrastructure_Does_Not_Depend_On_Api()
    {
        AssertNoDependency(_infrastructureAssembly, _apiNs);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static void AssertNoDependency(Assembly assembly, string forbiddenNamespace)
    {
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(forbiddenNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"assembly '{assembly.GetName().Name}' must not reference '{forbiddenNamespace}' " +
                     $"(Clean Architecture dependency rule). Failing types: " +
                     string.Join(", ", result.FailingTypeNames ?? []));
    }
}
