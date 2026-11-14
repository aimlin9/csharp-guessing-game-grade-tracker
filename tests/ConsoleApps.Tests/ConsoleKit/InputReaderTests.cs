using ConsoleKit;

namespace ConsoleApps.Tests;

/// <summary>
/// Tests for <see cref="InputReader"/>, using scripted text instead of a keyboard.
/// </summary>
public class InputReaderTests
{
    // Builds a reader fed by the given lines, and captures everything it prints.
    private static (InputReader Reader, StringWriter Output) CreateReader(params string[] lines)
    {
        var input = new StringReader(string.Join(Environment.NewLine, lines));
        var output = new StringWriter();

        return (new InputReader(input, output), output);
    }


    // ---------- Integers ----------

    [Fact]
    public void ReadInteger_ValidOnFirstTry_ReturnsValueAndShowsPrompt()
    {
        var (reader, output) = CreateReader("42");

        Assert.Equal(42, reader.ReadInteger("Guess: ", 1, 100));
        Assert.StartsWith("Guess: ", output.ToString());
    }

    [Fact]
    public void ReadInteger_NotANumber_AsksForWholeNumber()
    {
        var (reader, output) = CreateReader("abc", "7");

        Assert.Equal(7, reader.ReadInteger("Guess: ", 1, 100));
        Assert.Contains("Please enter a whole number.", output.ToString());
    }

    [Fact]
    public void ReadInteger_OutOfRange_ShowsRangeAndAsksAgain()
    {
        var (reader, output) = CreateReader("0", "101", "50");

        Assert.Equal(50, reader.ReadInteger("Guess: ", 1, 100));
        Assert.Contains("Please enter a number from 1 to 100.", output.ToString());
    }

    [Theory]
    [InlineData("1", 1)]       // lowest allowed
    [InlineData("100", 100)]   // highest allowed
    public void ReadInteger_RangeBoundaries_AreAccepted(string input, int expected)
    {
        var (reader, _) = CreateReader(input);

        Assert.Equal(expected, reader.ReadInteger("Guess: ", 1, 100));
    }

    [Fact]
    public void ReadInteger_MinGreaterThanMax_ThrowsBecauseItIsAProgrammingError()
    {
        var (reader, _) = CreateReader("5");

        Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadInteger("Guess: ", 10, 1));
    }


    // ---------- Decimals ----------

    [Fact]
    public void ReadDecimal_InvalidThenOutOfRangeThenValid_ShowsBothHints()
    {
        var (reader, output) = CreateReader("abc", "101", "87.5");

        var score = reader.ReadDecimal("Score: ", 0m, 100m);

        Assert.Equal(87.5m, score);
        Assert.Contains("That is not a valid number", output.ToString());
        Assert.Contains("Please enter a number from 0 to 100.", output.ToString());
    }


    // ---------- Yes / no ----------

    [Fact]
    public void ReadYesNo_UnrecognisedThenYes_AsksAgainThenReturnsTrue()
    {
        var (reader, output) = CreateReader("maybe", "y");

        Assert.True(reader.ReadYesNo("Play again? "));
        Assert.Contains("Please answer y or n.", output.ToString());
    }


    // ---------- Text ----------

    [Fact]
    public void ReadText_BlankThenName_ReturnsTrimmedName()
    {
        var (reader, output) = CreateReader("   ", "  Ama Owusu  ");

        Assert.Equal("Ama Owusu", reader.ReadText("Name: ", 50));
        Assert.Contains("Please enter some text (up to 50 characters).", output.ToString());
    }


    // ---------- Closed input ----------

    [Fact]
    public void AnyRead_WhenInputEnds_ThrowsInputEndedException()
    {
        var (reader, _) = CreateReader();

        Assert.Throws<InputEndedException>(() => reader.ReadInteger("Guess: ", 1, 100));
    }
}