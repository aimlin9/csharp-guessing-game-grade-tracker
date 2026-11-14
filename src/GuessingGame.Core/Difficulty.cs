namespace GuessingGame.Core;

/// <summary>
/// A difficulty level: how large the number range is and how many tries the player gets.
/// </summary>
/// <remarks>
/// The secret number is always between 1 and MaxNumber (inclusive).
///
/// The number of tries is chosen so every level can be won with a smart strategy.
/// Halving the range each guess (binary search) needs at most ceil(log2(MaxNumber + 1))
/// guesses: 5 for 1-20, 6 for 1-50 and 7 for 1-100. Easy and Medium give one spare
/// try; Hard gives exactly enough, so only careful play wins every time.
/// </remarks>
public sealed class Difficulty
{
    public static Difficulty Easy { get; } = new("Easy", maxNumber: 20, maxTries: 6);
    public static Difficulty Medium { get; } = new("Medium", maxNumber: 50, maxTries: 7);
    public static Difficulty Hard { get; } = new("Hard", maxNumber: 100, maxTries: 7);

    /// <summary>All built-in levels, easiest first. Used to build the difficulty menu.</summary>
    public static IReadOnlyList<Difficulty> All { get; } = [Easy, Medium, Hard];


    public string Name { get; }
    public int MaxNumber { get; }
    public int MaxTries { get; }


    /// <exception cref="ArgumentException">
    /// Thrown when the name is blank, the range has fewer than 2 numbers,
    /// or there are fewer than 1 tries.
    /// </exception>
    public Difficulty(string name, int maxNumber, int maxTries)
    {
        // A range of 1 would make the game pointless, and 0 tries makes it unwinnable.
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxNumber, 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTries, 1);

        Name = name;
        MaxNumber = maxNumber;
        MaxTries = maxTries;
    }


    /// <summary>A readable summary for menus, for example "Medium (1-50, 7 tries)".</summary>
    public override string ToString()
    {
        var triesWord = MaxTries == 1 ? "try" : "tries";
        return $"{Name} (1-{MaxNumber}, {MaxTries} {triesWord})";
    }
}