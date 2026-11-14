using GradeTracker.Cli;
using GradeTracker.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// End-to-end tests that run whole sessions through <see cref="TrackerApp"/>.
/// Each test lists exactly what a user would type, line by line.
/// </summary>
public class TrackerAppTests
{
    private static (int ExitCode, string Output) RunApp(params string[] lines) =>
        RunApp(GradeBook.DefaultCapacity, lines);

    private static (int ExitCode, string Output) RunApp(int capacity, params string[] lines)
    {
        var input = new StringReader(string.Join(Environment.NewLine, lines));
        var output = new StringWriter();

        var exitCode = new TrackerApp(input, output, capacity).Run();

        return (exitCode, output.ToString());
    }

    private static int CountOccurrences(string text, string value) => text.Split(value).Length - 1;


    // ---------- Menu ----------

    [Fact]
    public void Quit_Immediately_EndsCleanly()
    {
        var (exitCode, output) = RunApp("5");

        Assert.Equal(0, exitCode);
        Assert.Contains("Goodbye!", output);
    }

    [Fact]
    public void Menu_GarbageInput_AsksAgainAndNeverCrashes()
    {
        var (exitCode, output) = RunApp("abc", "0", "5");

        Assert.Contains("Please enter a whole number.", output);
        Assert.Contains("Please enter a number from 1 to 5.", output);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void GradeScale_ShowsEveryBand()
    {
        var (_, output) = RunApp("4", "5");

        Assert.Contains("A  70 to 100", output);
        Assert.Contains("B  60 to below 70", output);
        Assert.Contains("F  0 to below 40", output);
    }


    // ---------- Adding ----------

    [Fact]
    public void AddStudent_ConfirmsWithScoreAndGrade()
    {
        var (_, output) = RunApp("1", "Ama", "87.5", "n", "5");

        Assert.Contains("Added Ama: 87.5 (A)", output);
    }

    [Fact]
    public void AddStudent_BlankName_AsksAgain()
    {
        var (_, output) = RunApp("1", "   ", "Ama", "70", "n", "5");

        Assert.Contains("Please enter some text", output);
        Assert.Contains("Added Ama: 70 (A)", output);
    }

    [Fact]
    public void AddStudent_InvalidThenOutOfRangeScore_AsksAgainEachTime()
    {
        var (_, output) = RunApp("1", "Ama", "abc", "150", "75", "n", "5");

        Assert.Contains("That is not a valid number", output);
        Assert.Contains("Please enter a number from 0 to 100.", output);
        Assert.Contains("Added Ama: 75 (A)", output);
    }

    [Fact]
    public void AddStudent_DuplicateName_AsksForADifferentName()
    {
        var (_, output) = RunApp("1", "Ama", "70", "y", "ama", "Kofi", "60", "n", "5");

        Assert.Contains("ama is already in the grade book.", output);
        Assert.Contains("Added Kofi: 60 (B)", output);
    }

    [Fact]
    public void AddStudent_WhenFull_SaysSoAndStopsAsking()
    {
        // Capacity 2: the book fills after Kofi, then the user tries option 1 again.
        var (_, output) = RunApp(2, "1", "Ama", "70", "y", "Kofi", "60", "1", "5");

        Assert.Equal(2, CountOccurrences(output, "The grade book is full (2 students)."));
    }


    // ---------- Viewing ----------

    [Fact]
    public void ViewAll_ListsStudentsInEntryOrderWithGrades()
    {
        var (_, output) = RunApp("1", "Ama", "80", "y", "Kofi", "65.5", "n", "2", "5");

        var table = output[output.IndexOf("--- All students ---", StringComparison.Ordinal)..];

        Assert.True(
            table.IndexOf("Ama", StringComparison.Ordinal) < table.IndexOf("Kofi", StringComparison.Ordinal));
        Assert.Contains("65.5", table);
        Assert.Contains("2 of 30 places used.", table);
    }

    [Fact]
    public void ViewAll_WhenEmpty_ExplainsWhatToDo()
    {
        var (_, output) = RunApp("2", "5");

        Assert.Contains("No students yet.", output);
    }


    // ---------- Statistics ----------

    [Fact]
    public void Statistics_ShowAverageHighestAndLowest()
    {
        var (_, output) = RunApp(
            "1", "Ama", "80", "y", "Kofi", "65.5", "y", "Esi", "92", "n",
            "3", "5");

        // (80 + 65.5 + 92) / 3 = 79.1666..., shown as 79.17
        Assert.Contains("Average:  79.17 (A)", output);
        Assert.Contains("Highest:  92 (Esi, A)", output);
        Assert.Contains("Lowest:   65.5 (Kofi, B)", output);
    }

    [Fact]
    public void Statistics_WhenEmpty_ExplainsInsteadOfCrashing()
    {
        var (_, output) = RunApp("3", "5");

        Assert.Contains("No scores yet.", output);
    }


    // ---------- Closed input ----------

    [Fact]
    public void InputEndsMidEntry_ExitsCleanly()
    {
        var (exitCode, output) = RunApp("1", "Ama");

        Assert.Equal(0, exitCode);
        Assert.Contains("Input ended. Goodbye!", output);
    }
}