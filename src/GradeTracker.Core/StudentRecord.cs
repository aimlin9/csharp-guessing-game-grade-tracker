namespace GradeTracker.Core;

/// <summary>
/// One student's name and score. The letter grade is worked out from the score.
/// </summary>
/// <remarks>
/// A record is checked once, when it is created, and cannot change afterwards.
/// That means any StudentRecord that exists is guaranteed to be valid.
/// </remarks>
public sealed class StudentRecord
{
    /// <summary>Longest name allowed, so the student table stays readable.</summary>
    public const int MaxNameLength = 40;


    /// <exception cref="ArgumentException">Thrown when the name is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the score is outside 0 to 100.</exception>
    public StudentRecord(string name, decimal score)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedName.Length, MaxNameLength, nameof(name));

        GradeScale.EnsureValidScore(score);

        Name = trimmedName;
        Score = score;
    }


    public string Name { get; }

    public decimal Score { get; }

    /// <summary>Calculated from the score, so it can never disagree with it.</summary>
    public LetterGrade Grade => GradeScale.GradeFor(Score);
}