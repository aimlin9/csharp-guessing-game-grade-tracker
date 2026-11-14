using GradeTracker.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// Unit tests for <see cref="StudentRecord"/>.
/// </summary>
public class StudentRecordTests
{
    [Fact]
    public void Constructor_TrimsNameAndWorksOutGrade()
    {
        var student = new StudentRecord("  Ama Owusu  ", 87.5m);

        Assert.Equal("Ama Owusu", student.Name);
        Assert.Equal(87.5m, student.Score);
        Assert.Equal(LetterGrade.A, student.Grade);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankName_Throws(string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new StudentRecord(name, 50m));
    }

    [Fact]
    public void Constructor_NameLongerThanLimit_Throws()
    {
        var tooLong = new string('a', StudentRecord.MaxNameLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new StudentRecord(tooLong, 50m));
    }

    public static TheoryData<decimal> InvalidScores => new()
    {
        -1m,
        101m,
    };

    [Theory]
    [MemberData(nameof(InvalidScores))]
    public void Constructor_ScoreOutOfRange_Throws(decimal score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StudentRecord("Kofi", score));
    }
}