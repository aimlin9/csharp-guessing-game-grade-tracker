using GuessingGame.Core;

namespace ConsoleApps.Tests;

/// <summary>
/// A test stand-in for the random generator. Always returns the same secret number,
/// and remembers the range it was asked for, so tests can check that too.
/// </summary>
internal sealed class FixedSecretNumberGenerator(int secretNumber) : ISecretNumberGenerator
{
    public int LastMin { get; private set; }
    public int LastMax { get; private set; }

    public int Next(int minInclusive, int maxInclusive)
    {
        LastMin = minInclusive;
        LastMax = maxInclusive;

        return secretNumber;
    }
}