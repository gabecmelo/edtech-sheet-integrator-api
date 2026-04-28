using System.Diagnostics.CodeAnalysis;

namespace EdTech.SheetIntegrator.Application.Common;

/// <summary>
/// Discriminated success/failure container. Replaces throwing for expected business failures
/// (validation errors, not-found, conflicts) so use-case callers handle them as data.
/// Exceptions are reserved for genuinely exceptional paths.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Static factory methods (Success/Failure) are the idiomatic constructor pattern for generic Result types.")]
public sealed class Result<T>
{
    private readonly T _value;

    private Result(T value, Error? error)
    {
        _value = value;
        Error = error;
    }

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => Error is not null;

    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error) => new(default!, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
