namespace GradeTracker.Core;

/// <summary>
/// The minimum score needed for a letter grade.
/// </summary>
public sealed record GradeBand(LetterGrade Grade, decimal MinScore);


/// <summary>
/// Converts a score (0 to 100) into a letter grade.
/// </summary>
/// <remarks>
/// The scale follows common Ghanaian university grading:
///   A: 70 and above, B: 60 to 69, C: 50 to 59, D: 40 to 49, F: below 40.
///
/// The bands live in one ordered table, highest first, and the first band whose
/// minimum the score reaches wins. Changing the grading policy means editing
/// this table and nothing else. The console also prints this same table, so the
/// scale shown to users can never disagree with the scale actually used.
///
/// Scores are not rounded before grading: 69.99 is a B, not an A.
/// </remarks>
public static class GradeScale
{
    public const decimal MinScore = 0m;
    public const decimal MaxScore = 100m;

    /// <summary>The grade bands, from highest to lowest.</summary>
    public static IReadOnlyList<GradeBand> Bands { get; } =
    [
        new(LetterGrade.A, 70m),
        new(LetterGrade.B, 60m),
        new(LetterGrade.C, 50m),
        new(LetterGrade.D, 40m),
        new(LetterGrade.F, 0m),
    ];


    /// <summary>Returns the letter grade for a score.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the score is outside 0 to 100.</exception>
    public static LetterGrade GradeFor(decimal score)
    {
        EnsureValidScore(score);

        foreach (var band in Bands)
        {
            if (score >= band.MinScore)
            {
                return band.Grade;
            }
        }

        // Unreachable while the last band starts at 0, because every valid score is
        // at least 0. Kept so that a future edit leaving a gap fails loudly.
        throw new InvalidOperationException("The grade bands do not cover every valid score.");
    }


    /// <summary>Throws if the score is outside 0 to 100.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the score is out of range.</exception>
    public static void EnsureValidScore(decimal score)
    {
        if (score < MinScore || score > MaxScore)
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be between 0 and 100.");
        }
    }
}