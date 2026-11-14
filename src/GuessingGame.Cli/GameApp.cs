using ConsoleKit;

using GuessingGame.Core;

namespace GuessingGame.Cli;

/// <summary>
/// The guessing game's console app: a menu that repeats until the user quits.
/// </summary>
/// <remarks>
/// How responsibilities are split:
/// - GuessingRound (Core) owns the rules: secret number, tries and hints.
/// - InputReader (ConsoleKit) guarantees we only receive valid input.
/// - GameApp (this class) runs the menu and turns results into friendly messages.
///
/// Input, output and the number generator are all passed in. The real app uses
/// the console and a random generator; tests use scripted text and a fixed
/// secret number, so whole games can be played automatically.
/// </remarks>
public sealed class GameApp
{
    // Named menu options, so the switch below reads as words rather than magic numbers.
    private const int MenuPlay = 1;
    private const int MenuDifficulty = 2;
    private const int MenuHowToPlay = 3;
    private const int MenuQuit = 4;

    private readonly TextWriter _output;
    private readonly InputReader _reader;
    private readonly ISecretNumberGenerator _generator;

    // Medium is a fair starting point; the player can change it from the menu.
    private Difficulty _difficulty = Difficulty.Medium;

    // Simple session score, shown when the player quits.
    private int _roundsPlayed;
    private int _roundsWon;


    public GameApp(TextReader input, TextWriter output, ISecretNumberGenerator generator)
    {
        _output = output;
        _reader = new InputReader(input, output);
        _generator = generator;
    }


    /// <summary>
    /// Runs the menu until the player quits or the input stream ends.
    /// </summary>
    /// <returns>The process exit code. 0 means a normal, clean exit.</returns>
    public int Run()
    {
        WriteBanner();

        try
        {
            while (true)
            {
                WriteMainMenu();
                var choice = _reader.ReadInteger("Choose an option (1-4): ", 1, 4);
                _output.WriteLine();

                switch (choice)
                {
                    case MenuPlay:
                        PlayUntilDone();
                        break;

                    case MenuDifficulty:
                        ChooseDifficulty();
                        break;

                    case MenuHowToPlay:
                        ShowHowToPlay();
                        break;

                    case MenuQuit:
                        WriteSessionSummary();
                        _output.WriteLine("Goodbye!");
                        return 0;
                }

                _output.WriteLine();
            }
        }
        catch (InputEndedException)
        {
            // The input closed (Ctrl+Z, or piped input ran out). Leave politely.
            _output.WriteLine();
            _output.WriteLine("Input ended. Goodbye!");
            return 0;
        }
    }


    // ---------- Playing ----------

    // Plays one round, then keeps offering another until the player says no.
    private void PlayUntilDone()
    {
        do
        {
            PlayRound();
        }
        while (_reader.ReadYesNo("Play again? (y/n): "));
    }

    private void PlayRound()
    {
        var round = new GuessingRound(_difficulty, _generator);

        _output.WriteLine($"--- New round: {_difficulty} ---");
        _output.WriteLine($"I'm thinking of a number between 1 and {_difficulty.MaxNumber}.");

        while (!round.IsOver)
        {
            // InputReader enforces the range, so every guess reaching the engine is valid
            // and a mistyped guess never costs the player a try.
            var guess = _reader.ReadInteger(
                $"Your guess ({round.TriesLeft} {TriesWord(round.TriesLeft)} left): ",
                1,
                _difficulty.MaxNumber);

            WriteHint(round.Guess(guess));
        }

        _roundsPlayed++;

        if (round.IsWon)
        {
            _roundsWon++;
            _output.WriteLine($"  Correct! You guessed it in {round.TriesUsed} {TriesWord(round.TriesUsed)}.");
        }
        else
        {
            _output.WriteLine($"  Out of tries. The number was {round.RevealSecretNumber()}.");
        }
    }

    private void WriteHint(GuessResult result)
    {
        // A correct guess gets its own message once the round ends, in PlayRound.
        switch (result)
        {
            case GuessResult.TooLow:
                _output.WriteLine("  Too low. Try a higher number.");
                break;

            case GuessResult.TooHigh:
                _output.WriteLine("  Too high. Try a lower number.");
                break;
        }
    }


    // ---------- Other menu options ----------

    private void ChooseDifficulty()
    {
        _output.WriteLine("--- Choose difficulty ---");

        // The menu is built from Difficulty.All, so adding a new level to Core
        // makes it appear here automatically.
        for (var i = 0; i < Difficulty.All.Count; i++)
        {
            _output.WriteLine($"  {i + 1}. {Difficulty.All[i]}");
        }

        var choice = _reader.ReadInteger($"Difficulty (1-{Difficulty.All.Count}): ", 1, Difficulty.All.Count);
        _difficulty = Difficulty.All[choice - 1];

        _output.WriteLine($"  Difficulty set to {_difficulty}.");
    }

    private void ShowHowToPlay()
    {
        _output.WriteLine("--- How to play ---");
        _output.WriteLine("  I pick a secret number. After each guess I tell you whether");
        _output.WriteLine("  the secret number is higher or lower. Find it before you run out of tries.");
        _output.WriteLine("  Tip: guess the middle of the range that is left. It halves your search each time.");
    }


    // ---------- Output helpers ----------

    private void WriteBanner()
    {
        _output.WriteLine("==============================");
        _output.WriteLine("    Number Guessing Game");
        _output.WriteLine("==============================");
        _output.WriteLine();
    }

    private void WriteMainMenu()
    {
        _output.WriteLine("Main menu");
        _output.WriteLine("  1. Play");
        _output.WriteLine($"  2. Change difficulty [current: {_difficulty}]");
        _output.WriteLine("  3. How to play");
        _output.WriteLine("  4. Quit");
    }

    private void WriteSessionSummary()
    {
        // Nothing to report if the player never played.
        if (_roundsPlayed == 0)
        {
            return;
        }

        var roundsWord = _roundsPlayed == 1 ? "round" : "rounds";
        _output.WriteLine($"You won {_roundsWon} of {_roundsPlayed} {roundsWord}.");
    }

    private static string TriesWord(int count) => count == 1 ? "try" : "tries";
}