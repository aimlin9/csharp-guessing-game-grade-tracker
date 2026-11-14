namespace GuessingGame.Core;

/// <summary>
/// Supplies the secret number for a round.
/// </summary>
/// <remarks>
/// This exists so the game does not depend on randomness directly.
/// The app uses <see cref="RandomSecretNumberGenerator"/>; tests use a generator
/// that always returns a known number, so every test is predictable and repeatable.
/// </remarks>
public interface ISecretNumberGenerator
{
    /// <summary>Returns a number between minInclusive and maxInclusive, both included.</summary>
    int Next(int minInclusive, int maxInclusive);
}