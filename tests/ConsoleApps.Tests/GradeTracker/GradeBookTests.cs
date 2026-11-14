using GradeTracker.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// Unit tests for <see cref="GradeBook"/>.
/// </summary>
public class GradeBookTests
{
    // ---------- Capacity ----------

    [Fact]
    public void NewGradeBook_IsEmptyWithDefaultCapacity()
    {
        var book = new GradeBook();

        Assert.Equal(0, book.Count);
        Assert.Equal(30, book.Capacity);
        Assert.True(book.IsEmpty);
        Assert.False(book.IsFull);
    }

    [Fact]
    public void NewGradeBook_CustomCapacity_IsUsed()
    {
        Assert.Equal(5, new GradeBook(5).Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_CapacityBelowOne_Throws(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GradeBook(capacity));
    }


    // ---------- Adding ----------

    [Fact]
    public void Add_StoresStudentAndReturnsRecordWithGrade()
    {
        var book = new GradeBook();

        var added = book.Add("Ama", 72m);

        Assert.Equal(1, book.Count);
        Assert.Equal("Ama", added.Name);
        Assert.Equal(LetterGrade.A, added.Grade);
    }

    [Fact]
    public void Add_WhenFull_ThrowsWithClearMessage()
    {
        var book = new GradeBook(capacity: 2);
        book.Add("Ama", 70m);
        book.Add("Kofi", 60m);

        var ex = Assert.Throws<InvalidOperationException>(() => book.Add("Esi", 50m));

        Assert.True(book.IsFull);
        Assert.Equal("The grade book is full (2 students).", ex.Message);
    }

    [Fact]
    public void Add_DuplicateNameIgnoringCaseAndSpaces_Throws()
    {
        var book = new GradeBook();
        book.Add("Ama", 70m);

        var ex = Assert.Throws<ArgumentException>(() => book.Add("  ama ", 80m));

        Assert.Equal("A student named ama is already in the grade book.", ex.Message);
        Assert.Equal(1, book.Count);
    }

    [Fact]
    public void Add_InvalidScore_ThrowsAndStoresNothing()
    {
        var book = new GradeBook();

        Assert.Throws<ArgumentOutOfRangeException>(() => book.Add("Ama", 120m));
        Assert.True(book.IsEmpty);
    }


    // ---------- Reading back ----------

    [Fact]
    public void GetStudents_ReturnsStudentsInTheOrderAdded()
    {
        var book = new GradeBook();
        book.Add("Ama", 70m);
        book.Add("Kofi", 60m);

        var names = book.GetStudents().Select(s => s.Name);

        Assert.Equal(["Ama", "Kofi"], names);
    }

    [Fact]
    public void GetStudents_ReturnsACopy_SoTheGradeBookCannotBeChangedFromOutside()
    {
        var book = new GradeBook();
        book.Add("Ama", 70m);

        var copy = book.GetStudents();
        copy[0] = new StudentRecord("Intruder", 0m);

        Assert.Equal("Ama", book.GetStudents()[0].Name);
    }


    // ---------- Statistics ----------

    [Fact]
    public void GetStatistics_CalculatesAverageHighestAndLowest()
    {
        var book = new GradeBook();
        book.Add("Ama", 80m);
        book.Add("Kofi", 65.5m);
        book.Add("Esi", 92m);

        var stats = book.GetStatistics();

        // (80 + 65.5 + 92) / 3 = 79.1666...
        Assert.Equal(3, stats.Count);
        Assert.Equal(79.17m, Math.Round(stats.Average, 2));
        Assert.Equal("Esi", stats.Highest.Name);
        Assert.Equal("Kofi", stats.Lowest.Name);
        Assert.Equal(LetterGrade.A, stats.AverageGrade);
    }

    [Fact]
    public void GetStatistics_OneStudent_IsAverageHighestAndLowest()
    {
        var book = new GradeBook();
        book.Add("Ama", 55m);

        var stats = book.GetStatistics();

        Assert.Equal(55m, stats.Average);
        Assert.Equal("Ama", stats.Highest.Name);
        Assert.Equal("Ama", stats.Lowest.Name);
    }

    [Fact]
    public void GetStatistics_TiedScores_KeepTheFirstStudentEntered()
    {
        var book = new GradeBook();
        book.Add("Ama", 90m);
        book.Add("Kofi", 90m);

        var stats = book.GetStatistics();

        Assert.Equal("Ama", stats.Highest.Name);
        Assert.Equal("Ama", stats.Lowest.Name);
    }

    [Fact]
    public void GetStatistics_Empty_ThrowsInsteadOfDividingByZero()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new GradeBook().GetStatistics());

        Assert.Equal("No scores have been entered yet.", ex.Message);
    }
}