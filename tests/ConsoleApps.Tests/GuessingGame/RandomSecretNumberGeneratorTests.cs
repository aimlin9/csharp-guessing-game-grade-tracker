using GuessingGame.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// Tests for <see cref="RandomSecretNumberGenerator"/>.
/// Randomness cannot be tested for exact values, so these check the properties
/// that matter: every result is in range, and both ends of the range can come up.
/// </summary>
public class RandomSecretNumberGeneratorTests
{
    private readonly RandomSecretNumberGenerator _generator = new();

    [Fact]
    public void Next_ManyDraws_StayInRangeAndCoverEveryValue()
    {
        // 1,000 draws from 1-10. The chance of any value never appearing is
        // around 1 in 10^44, so this test will not fail by bad luck.
        var draws = Enumerable.Range(0, 1000).Select(_ => _generator.Next(1, 10)).ToList();

        Assert.All(draws, number => Assert.InRange(number, 1, 10));
        Assert.Equal(10, draws.Distinct().Count());
    }

    [Fact]
    public void Next_MinEqualsMax_ReturnsThatValue()
    {
        Assert.Equal(7, _generator.Next(7, 7));
    }

    [Fact]
    public void Next_MinGreaterThanMax_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Next(10, 1));
    }
}