namespace GradeTracker.Core;

/// <summary>
/// A summary of the class: how many students, the average, and the highest and lowest scorers.
/// </summary>
/// <param name="Count">Number of students included.</param>
/// <param name="Average">Mean score at full precision. Round it only for display.</param>
/// <param name="Highest">The student with the highest score (the first one entered, if tied).</param>
/// <param name="Lowest">The student with the lowest score (the first one entered, if tied).</param>
public sealed record GradeStatistics(int Count, decimal Average, StudentRecord Highest, StudentRecord Lowest)
{
    /// <summary>The letter grade the class average would earn.</summary>
    public LetterGrade AverageGrade => GradeScale.GradeFor(Average);
}