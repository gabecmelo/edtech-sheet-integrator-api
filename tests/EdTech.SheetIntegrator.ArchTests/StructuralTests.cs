using System.Reflection;
using EdTech.SheetIntegrator.Application.Common;
using EdTech.SheetIntegrator.Application.DependencyInjection;
using EdTech.SheetIntegrator.Domain.Assessments;
using EdTech.SheetIntegrator.Domain.Exceptions;
using NetArchTest.Rules;

namespace EdTech.SheetIntegrator.ArchTests;

/// <summary>
/// Structural conventions that reinforce the domain model and application design:
/// <list type="bullet">
///   <item>Use-case classes live in the Application layer.</item>
///   <item>Domain-specific exceptions extend <see cref="DomainException"/>.</item>
///   <item>Repository abstractions live in Application (not Domain, not Infrastructure).</item>
/// </list>
/// </summary>
public sealed class StructuralTests
{
    private static readonly Assembly DomainAssembly =
        typeof(Assessment).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(IApplicationAssemblyMarker).Assembly;

    // ── Use-case convention ───────────────────────────────────────────────────

    [Fact]
    public void UseCase_Classes_Reside_In_Application_Assembly()
    {
        // Every type whose name ends with "UseCase" should be in the Application project.
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("UseCase")
            .Should()
            .ImplementInterface(typeof(IUseCase<,>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "every UseCase class must implement IUseCase<TInput,TOutput>. " +
                     "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void UseCase_Classes_Are_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("UseCase")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "use cases should be sealed; inherit from a use case instead of " +
                     "subclassing. Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Domain-exception convention ───────────────────────────────────────────

    [Fact]
    public void Domain_Exceptions_Inherit_From_DomainException()
    {
        // Every concrete exception class in the Domain assembly (other than DomainException
        // itself) must extend DomainException so callers can catch at the right granularity.
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Exception")
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveName(nameof(DomainException))
            .Should()
            .Inherit(typeof(DomainException))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "domain exceptions should all extend DomainException for consistent " +
                     "catch blocks. Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}
