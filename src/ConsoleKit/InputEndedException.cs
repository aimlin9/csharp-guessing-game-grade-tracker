namespace ConsoleKit;

/// <summary>
/// Signals that no more input will ever arrive.
/// </summary>
/// <remarks>
/// This happens when the user presses Ctrl+Z then Enter on Windows (Ctrl+D on
/// Linux or macOS), or when piped input runs out. Console.ReadLine() then returns
/// null forever, so without this the "ask again" loop would spin endlessly.
/// Each app catches this exception and exits cleanly.
/// </remarks>
public sealed class InputEndedException : Exception
{
    public InputEndedException()
        : base("Input ended.")
    {
    }
}