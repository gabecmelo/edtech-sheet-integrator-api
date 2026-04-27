namespace EdTech.SheetIntegrator.Domain.Grading;

/// <summary>How a student response should be compared against a question's correct answer.</summary>
public enum MatchMode
{
    /// <summary>Byte-for-byte ordinal match. Whitespace and case matter.</summary>
    Exact = 0,

    /// <summary>Trimmed, culture-invariant, case-insensitive comparison.</summary>
    CaseInsensitive = 1,

    /// <summary>Numeric comparison with an absolute tolerance.</summary>
    Numeric = 2,
}
