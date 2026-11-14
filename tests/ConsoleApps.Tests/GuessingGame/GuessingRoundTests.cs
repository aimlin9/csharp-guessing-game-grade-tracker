using GuessingGame.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// Unit tests for <see cref="GuessingRound"/>.
/// Every round here uses Hard (1-100, 7 tries) with a secret number of 42.
/// </summary>
public class GuessingRoundTests
{
    private const int Secret = 42;

    private static GuessingRound NewRound() =>
        new(Difficulty.Hard, new FixedSecretNumberGenerator(Secret));


    // ---------- Starting state ----------

    [Fact]
    public void NewRound_StartsWithAllTriesAndIsNotOver()
    {
        var round = NewRound();

        Assert.Equal(0, round.TriesUsed);
        Assert.Equal(7, round.TriesLeft);
        Assert.False(round.IsOver);
        Assert.False(round.IsWon);
    }

    [Fact]
    public void NewRound_AsksGeneratorForTheDifficultysRange()
    {
        var generator = new FixedSecretNumberGenerator(Secret);

        _ = new GuessingRound(Difficulty.Hard, generator);

        Assert.Equal(1, generator.LastMin);
        Assert.Equal(100, generator.LastMax);
    }


    // ---------- Hints ----------

    [Theory]
    [InlineData(10, GuessResult.TooLow)]
    [InlineData(90, GuessResult.TooHigh)]
    public void Guess_Wrong_GivesHintAndUsesOneTry(int guess, GuessResult expected)
    {
        var round = NewRound();

        Assert.Equal(expected, round.Guess(guess));
        Assert.Equal(1, round.TriesUsed);
        Assert.Equal(6, round.TriesLeft);
        Assert.False(round.IsOver);
    }

    [Fact]
    public void Guess_Correct_WinsAndEndsTheRound()
    {
        var round = NewRound();

        Assert.Equal(GuessResult.Correct, round.Guess(Secret));
        Assert.True(round.IsWon);
        Assert.True(round.IsOver);
    }


    // ---------- Running out of tries ----------

    [Fact]
    public void Guess_AllTriesWrong_EndsTheRoundAsALoss()
    {
        var round = NewRound();

        for (var i = 0; i < 7; i++)
        {
            round.Guess(1);
        }

        Assert.True(round.IsOver);
        Assert.False(round.IsWon);
        Assert.Equal(0, round.TriesLeft);
    }

    [Fact]
    public void Guess_CorrectOnTheLastTry_StillCountsAsAWin()
    {
        var round = NewRound();

        for (var i = 0; i < 6; i++)
        {
            round.Guess(1);
        }

        Assert.Equal(GuessResult.Correct, round.Guess(Secret));
        Assert.True(round.IsWon);
        Assert.Equal(0, round.TriesLeft);
    }

    [Fact]
    public void Guess_AfterTheRoundIsOver_Throws()
    {
        var round = NewRound();
        round.Guess(Secret);

        Assert.Throws<InvalidOperationException>(() => round.Guess(Secret));
    }


    // ---------- Invalid guesses ----------

    [Theory]
    [InlineData(0)]     // just below the range
    [InlineData(101)]   // just above the range
    public void Guess_OutOfRange_ThrowsAndDoesNotUseATry(int guess)
    {
        var round = NewRound();

        Assert.Throws<ArgumentOutOfRangeException>(() => round.Guess(guess));
        Assert.Equal(0, round.TriesUsed);
    }


    // ---------- Revealing the secret ----------

    [Fact]
    public void RevealSecretNumber_WhileInProgress_Throws()
    {
        var round = NewRound();

        Assert.Throws<InvalidOperationException>(() => round.RevealSecretNumber());
    }

    [Fact]
    public void RevealSecretNumber_AfterLosing_ReturnsTheSecret()
    {
        var round = NewRound();

        for (var i = 0; i < 7; i++)
        {
            round.Guess(1);
        }

        Assert.Equal(Secret, round.RevealSecretNumber());
    }


    // ---------- Faulty generator ----------

    [Fact]
    public void Constructor_GeneratorOutOfRange_Throws()
    {
        var faulty = new FixedSecretNumberGenerator(0);

        Assert.Throws<InvalidOperationException>(() => new GuessingRound(Difficulty.Hard, faulty));
    }
}