using GradeTracker.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// Unit tests for <see cref="GradeScale"/>.
/// Boundaries are where grading bugs hide, so every band is tested at its
/// minimum and just below it.
/// </summary>
public class GradeScaleTests
{
    public static TheoryData<decimal, LetterGrade> BoundaryCases => new()
    {
        { 100m, LetterGrade.A },
        { 70m, LetterGrade.A },      // lowest A
        { 69.99m, LetterGrade.B },   // just below A, no rounding up
        { 60m, LetterGrade.B },
        { 59.99m, LetterGrade.C },
        { 50m, LetterGrade.C },
        { 49.99m, LetterGrade.D },
        { 40m, LetterGrade.D },
        { 39.99m, LetterGrade.F },
        { 0m, LetterGrade.F },
    };

    public static TheoryData<decimal> OutOfRangeScores => new()
    {
        -0.01m,
        100.01m,
    };


    [Theory]
    [MemberData(nameof(BoundaryCases))]
    public void GradeFor_BandBoundaries_ReturnExpectedGrade(decimal score, LetterGrade expected)
    {
        Assert.Equal(expected, GradeScale.GradeFor(score));
    }

    [Theory]
    [MemberData(nameof(OutOfRangeScores))]
    public void GradeFor_ScoreOutsideZeroToHundred_Throws(decimal score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GradeScale.GradeFor(score));
    }

    [Fact]
    public void Bands_RunFromAAtSeventyDownToFAtZero()
    {
        Assert.Equal(new GradeBand(LetterGrade.A, 70m), GradeScale.Bands[0]);
        Assert.Equal(new GradeBand(LetterGrade.F, 0m), GradeScale.Bands[^1]);
    }
}