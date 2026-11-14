using System.Globalization;

namespace ConsoleKit;

/// <summary>
/// Asks the user for input and keeps asking until the answer is valid.
/// </summary>
/// <remarks>
/// Bad input never throws. The user sees a short hint saying exactly what was
/// wrong (not a number, or out of range) and is asked again.
///
/// The reader and writer are passed in rather than using Console directly.
/// The apps pass Console.In and Console.Out; tests pass in-memory text, so whole
/// conversations can be scripted and checked automatically.
/// </remarks>
public sealed class InputReader
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public InputReader(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
    }


    /// <summary>Asks for a whole number between min and max (inclusive).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when min is greater than max.</exception>
    public int ReadInteger(string prompt, int min, int max)
    {
        // An impossible range is a programming mistake, so fail loudly
        // instead of trapping the user in a loop no answer can escape.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

        while (true)
        {
            var text = ReadLineOrThrow(prompt);

            if (!InputParser.TryParseInteger(text, out var value))
            {
                _output.WriteLine("  Please enter a whole number.");
                continue;
            }

            if (value < min || value > max)
            {
                _output.WriteLine($"  Please enter a number from {min} to {max}.");
                continue;
            }

            return value;
        }
    }


    /// <summary>Asks for a decimal number between min and max (inclusive).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when min is greater than max.</exception>
    public decimal ReadDecimal(string prompt, decimal min, decimal max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

        while (true)
        {
            var text = ReadLineOrThrow(prompt);

            if (!InputParser.TryParseDecimal(text, out var value))
            {
                _output.WriteLine("  That is not a valid number. Use digits and a dot for decimals, for example 87.5");
                continue;
            }

            if (value < min || value > max)
            {
                _output.WriteLine($"  Please enter a number from {Format(min)} to {Format(max)}.");
                continue;
            }

            return value;
        }
    }


    /// <summary>Asks a yes/no question. Returns true for yes.</summary>
    public bool ReadYesNo(string prompt)
    {
        while (true)
        {
            var text = ReadLineOrThrow(prompt);

            if (InputParser.TryParseYesNo(text, out var answer))
            {
                return answer;
            }

            _output.WriteLine("  Please answer y or n.");
        }
    }


    /// <summary>Asks for non-empty text up to maxLength characters, trimmed.</summary>
    public string ReadText(string prompt, int maxLength)
    {
        while (true)
        {
            var text = ReadLineOrThrow(prompt);

            if (InputParser.TryParseText(text, maxLength, out var result))
            {
                return result;
            }

            _output.WriteLine($"  Please enter some text (up to {maxLength} characters).");
        }
    }


    // Writes the prompt on the same line as the user's answer, then reads it.
    // A null line means the input stream has closed for good.
    private string ReadLineOrThrow(string prompt)
    {
        _output.Write(prompt);

        return _input.ReadLine() ?? throw new InputEndedException();
    }

    // Invariant formatting so range hints look the same on every machine.
    private static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}