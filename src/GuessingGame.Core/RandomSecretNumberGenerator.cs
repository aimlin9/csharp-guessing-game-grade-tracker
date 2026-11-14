namespace GuessingGame.Core;

/// <summary>
/// Picks secret numbers at random. This is the generator the real game uses.
/// </summary>
public sealed class RandomSecretNumberGenerator : ISecretNumberGenerator
{
    /// <exception cref="ArgumentOutOfRangeException">Thrown when min is greater than max.</exception>
    public int Next(int minInclusive, int maxInclusive)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minInclusive, maxInclusive);

        // Random.Next treats its upper bound as exclusive, hence the + 1.
        //
        // Random.Shared is a single, thread-safe instance seeded once. It avoids a
        // classic bug where creating "new Random()" repeatedly in quick succession
        // can produce the same sequence of numbers.
        return Random.Shared.Next(minInclusive, maxInclusive + 1);
    }
}