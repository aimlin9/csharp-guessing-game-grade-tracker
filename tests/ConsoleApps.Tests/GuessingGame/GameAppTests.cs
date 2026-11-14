using GuessingGame.Cli;

namespace ConsoleApps.Tests;

/// <summary>
/// End-to-end tests that play whole sessions through <see cref="GameApp"/>.
/// </summary>
/// <remarks>
/// The secret number is fixed at 5, which is valid on every difficulty.
/// The default difficulty is Medium (1-50, 7 tries).
/// Each test lists exactly what a player would type, line by line.
/// </remarks>
public class GameAppTests
{
    private const int Secret = 5;

    private static (int ExitCode, string Output) RunApp(params string[] lines)
    {
        var input = new StringReader(string.Join(Environment.NewLine, lines));
        var output = new StringWriter();

        var exitCode = new GameApp(input, output, new FixedSecretNumberGenerator(Secret)).Run();

        return (exitCode, output.ToString());
    }


    // ---------- Menu ----------

    [Fact]
    public void Quit_Immediately_EndsCleanlyWithoutSummary()
    {
        var (exitCode, output) = RunApp("4");

        Assert.Equal(0, exitCode);
        Assert.Contains("Goodbye!", output);
        Assert.DoesNotContain("You won", output);
    }

    [Fact]
    public void Menu_GarbageInput_AsksAgainAndNeverCrashes()
    {
        var (exitCode, output) = RunApp("abc", "9", "4");

        Assert.Contains("Please enter a whole number.", output);
        Assert.Contains("Please enter a number from 1 to 4.", output);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void HowToPlay_ExplainsTheRules()
    {
        var (_, output) = RunApp("3", "4");

        Assert.Contains("higher or lower", output);
    }


    // ---------- Playing ----------

    [Fact]
    public void Play_HintsThenWin_ShowsHintsAndTryCount()
    {
        // Guess 10 (too high), then 2 (too low), then 5 (correct).
        var (_, output) = RunApp("1", "10", "2", "5", "n", "4");

        Assert.Contains("Too high.", output);
        Assert.Contains("Too low.", output);
        Assert.Contains("Correct! You guessed it in 3 tries.", output);
    }

    [Fact]
    public void Play_WinOnFirstGuess_UsesSingularTry()
    {
        var (_, output) = RunApp("1", "5", "n", "4");

        Assert.Contains("You guessed it in 1 try.", output);
        Assert.Contains("You won 1 of 1 round.", output);
    }

    [Fact]
    public void Play_RunOutOfTries_RevealsTheNumber()
    {
        // Seven wrong guesses on Medium uses every try.
        var (_, output) = RunApp("1", "1", "1", "1", "1", "1", "1", "1", "n", "4");

        Assert.Contains("Out of tries. The number was 5.", output);
        Assert.Contains("You won 0 of 1 round.", output);
    }

    [Fact]
    public void PlayAgain_Yes_StartsANewRound()
    {
        var (_, output) = RunApp("1", "5", "y", "5", "n", "4");

        var wins = output.Split("Correct!").Length - 1;

        Assert.Equal(2, wins);
        Assert.Contains("You won 2 of 2 rounds.", output);
    }

    [Fact]
    public void PlayAgain_UnclearAnswer_AsksAgain()
    {
        var (_, output) = RunApp("1", "5", "maybe", "n", "4");

        Assert.Contains("Please answer y or n.", output);
    }


    // ---------- Difficulty ----------

    [Fact]
    public void ChangeDifficulty_ToEasy_AppliesNewRangeAndRejectsOutOfRangeGuess()
    {
        // Switch to Easy (1-20), guess 21 (rejected, costs nothing), then 5.
        var (_, output) = RunApp("2", "1", "1", "21", "5", "n", "4");

        Assert.Contains("Difficulty set to Easy (1-20, 6 tries).", output);
        Assert.Contains("Please enter a number from 1 to 20.", output);
        Assert.Contains("You guessed it in 1 try.", output);
    }


    // ---------- Closed input ----------

    [Fact]
    public void InputEndsMidRound_ExitsCleanly()
    {
        var (exitCode, output) = RunApp("1", "10");

        Assert.Equal(0, exitCode);
        Assert.Contains("Input ended. Goodbye!", output);
    }
}