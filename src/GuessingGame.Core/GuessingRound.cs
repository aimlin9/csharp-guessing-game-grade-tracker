namespace GuessingGame.Core;

/// <summary>
/// One round of the guessing game: a secret number and a limited number of tries.
/// </summary>
/// <remarks>
/// Rules:
/// - Each valid guess uses one try and gets a hint: too low, too high, or correct.
/// - The round ends when the player guesses correctly or runs out of tries.
/// - A guess outside the allowed range is rejected and does NOT use a try.
///   The console prevents this anyway; the check here is a safety net.
/// - The secret number stays hidden until the round is over.
///
/// This class has no console code, so every rule is unit tested directly.
/// </remarks>
public sealed class GuessingRound
{
    private readonly int _secretNumber;


    /// <exception cref="InvalidOperationException">
    /// Thrown when the generator returns a number outside the difficulty's range.
    /// </exception>
    public GuessingRound(Difficulty difficulty, ISecretNumberGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(difficulty);
        ArgumentNullException.ThrowIfNull(generator);

        Difficulty = difficulty;
        _secretNumber = generator.Next(1, difficulty.MaxNumber);

        // Never trust a dependency blindly: a faulty generator would otherwise
        // create a round that can never be won.
        if (_secretNumber < 1 || _secretNumber > difficulty.MaxNumber)
        {
            throw new InvalidOperationException(
                $"The number generator returned {_secretNumber}, which is outside 1 to {difficulty.MaxNumber}.");
        }
    }


    public Difficulty Difficulty { get; }

    /// <summary>How many valid guesses have been made so far.</summary>
    public int TriesUsed { get; private set; }

    /// <summary>How many guesses are left before the round is lost.</summary>
    public int TriesLeft => Difficulty.MaxTries - TriesUsed;

    /// <summary>True once the player has guessed the secret number.</summary>
    public bool IsWon { get; private set; }

    /// <summary>True once the round is won or there are no tries left.</summary>
    public bool IsOver => IsWon || TriesLeft == 0;


    /// <summary>
    /// Makes a guess and returns a hint.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the round is already over.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the guess is outside 1 to MaxNumber.</exception>
    public GuessResult Guess(int guess)
    {
        if (IsOver)
        {
            throw new InvalidOperationException("The round is already over.");
        }

        // Validate before counting the try, so a rejected guess costs nothing.
        ArgumentOutOfRangeException.ThrowIfLessThan(guess, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(guess, Difficulty.MaxNumber);

        TriesUsed++;

        if (guess == _secretNumber)
        {
            IsWon = true;
            return GuessResult.Correct;
        }

        return guess < _secretNumber ? GuessResult.TooLow : GuessResult.TooHigh;
    }


    /// <summary>
    /// Returns the secret number, but only once the round is over.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown while the round is still in progress.</exception>
    public int RevealSecretNumber()
    {
        if (!IsOver)
        {
            throw new InvalidOperationException("The secret number can only be revealed once the round is over.");
        }

        return _secretNumber;
    }
}