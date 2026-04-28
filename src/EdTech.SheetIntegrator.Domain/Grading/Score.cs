using EdTech.SheetIntegrator.Domain.Exceptions;

namespace EdTech.SheetIntegrator.Domain.Grading;

/// <summary>
/// Immutable value object representing how many points a student earned out of the maximum
/// possible. Invariant: 0 &lt;= Earned &lt;= Total and Total &gt; 0.
/// </summary>
public readonly record struct Score
{
    public decimal Earned { get; }
    
    public decimal Total { get; }

    public Score(decimal earned, decimal total)
    {
        if (total <= 0m)
        {
            throw new DomainException("Score total must be greater than zero.");
        }

        if (earned < 0m)
        {
            throw new DomainException("Earned points cannot be negative.");
        }

        if (earned > total)
        {
            throw new DomainException("Earned points cannot exceed total points.");
        }

        Earned = earned;
        Total = total;
    }

    /// <summary>Percentage from 0 to 100, rounded to two decimals (banker's rounding away from zero).</summary>
    public decimal Percentage => Math.Round(Earned / Total * 100m, 2, MidpointRounding.AwayFromZero);

    public static Score Zero(decimal total) => new(0m, total);
}
