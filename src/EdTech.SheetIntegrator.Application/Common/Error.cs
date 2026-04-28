using System.Diagnostics.CodeAnalysis;

namespace EdTech.SheetIntegrator.Application.Common;

/// <summary>
/// Stable, machine-readable failure descriptor returned from a use case.
/// <see cref="Code"/> is intended for programmatic dispatch (e.g. mapping to an HTTP status);
/// <see cref="Message"/> is human-readable detail.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Error is the clearest name for this concept. No VB.NET consumers are planned.")]
public sealed record Error(string Code, string Message);
