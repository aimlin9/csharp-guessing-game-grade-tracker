using GuessingGame.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// Unit tests for <see cref="Difficulty"/>.
/// </summary>
public class DifficultyTests
{
    [Fact]
    public void Presets_HaveExpectedRangesAndTries()
    {
        Assert.Equal(20, Difficulty.Easy.MaxNumber);
        Assert.Equal(6, Difficulty.Easy.MaxTries);

        Assert.Equal(50, Difficulty.Medium.MaxNumber);
        Assert.Equal(7, Difficulty.Medium.MaxTries);

        Assert.Equal(100, Difficulty.Hard.MaxNumber);
        Assert.Equal(7, Difficulty.Hard.MaxTries);
    }

    [Fact]
    public void All_ListsThreeLevelsEasiestFirst()
    {
        Assert.Equal([Difficulty.Easy, Difficulty.Medium, Difficulty.Hard], Difficulty.All);
    }

    [Theory]
    [InlineData("", 20, 5)]        // blank name
    [InlineData("Tiny", 1, 5)]     // a range of one number is not a game
    [InlineData("Cruel", 20, 0)]   // zero tries can never be won
    public void Constructor_InvalidSettings_Throws(string name, int maxNumber, int maxTries)
    {
        // ThrowsAny, because ArgumentOutOfRangeException is a kind of ArgumentException.
        Assert.ThrowsAny<ArgumentException>(() => new Difficulty(name, maxNumber, maxTries));
    }

    [Fact]
    public void ToString_ShowsNameRangeAndTries()
    {
        Assert.Equal("Medium (1-50, 7 tries)", Difficulty.Medium.ToString());
    }

    [Fact]
    public void ToString_SingleTry_UsesSingularWord()
    {
        var suddenDeath = new Difficulty("Sudden death", 10, 1);

        Assert.Equal("Sudden death (1-10, 1 try)", suddenDeath.ToString());
    }
}