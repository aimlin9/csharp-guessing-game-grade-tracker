namespace GuessingGame.Core;

/// <summary>
/// What the game says back after a guess.
/// </summary>
public enum GuessResult
{
    /// <summary>The guess is smaller than the secret number.</summary>
    TooLow,

    /// <summary>The guess is larger than the secret number.</summary>
    TooHigh,

    /// <summary>The guess matches the secret number.</summary>
    Correct
}