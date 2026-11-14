using System.Globalization;

namespace ConsoleKit;

/// <summary>
/// Turns text typed by a user into typed values, without ever throwing.
/// </summary>
/// <remarks>
/// Shared by both apps, so the "never crash on bad input" rules are written
/// and tested exactly once.
///
/// This class only checks the format of the input. Whether a value is in an
/// allowed range (a score between 0 and 100, a guess between 1 and 100) is
/// checked by <see cref="InputReader"/>, so the user can be told precisely
/// what was wrong.
///
/// Number rules (same as Assignment 1):
/// - Spaces around the input are ignored, and a leading + or - is allowed.
/// - The decimal point must be a dot. Commas are rejected, because "1,500"
///   could mean fifteen hundred or one and a half.
/// - InvariantCulture is used so input is read the same way on every machine,
///   whatever its regional settings.
/// </remarks>
public static class InputParser
{
    private const NumberStyles DecimalRules =
        NumberStyles.AllowLeadingWhite |
        NumberStyles.AllowTrailingWhite |
        NumberStyles.AllowLeadingSign |
        NumberStyles.AllowDecimalPoint;

    // Whole numbers: same as above, minus the decimal point.
    private const NumberStyles IntegerRules =
        NumberStyles.AllowLeadingWhite |
        NumberStyles.AllowTrailingWhite |
        NumberStyles.AllowLeadingSign;


    /// <summary>Tries to read a decimal number, such as a score of 87.5.</summary>
    public static bool TryParseDecimal(string? input, out decimal value)
    {
        // Returns false (never throws) for null, empty, malformed or oversized input.
        return decimal.TryParse(input, DecimalRules, CultureInfo.InvariantCulture, out value);
    }


    /// <summary>Tries to read a whole number, such as a guess or a menu choice.</summary>
    public static bool TryParseInteger(string? input, out int value)
    {
        return int.TryParse(input, IntegerRules, CultureInfo.InvariantCulture, out value);
    }


    /// <summary>
    /// Tries to read a yes/no answer. Accepts y, yes, n and no in any letter case.
    /// </summary>
    public static bool TryParseYesNo(string? input, out bool answer)
    {
        switch (input?.Trim().ToLowerInvariant())
        {
            case "y":
            case "yes":
                answer = true;
                return true;

            case "n":
            case "no":
                answer = false;
                return true;

            default:
                answer = false;
                return false;
        }
    }


    /// <summary>
    /// Tries to read non-empty text up to a maximum length, such as a student name.
    /// Surrounding spaces are trimmed off.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is zero or negative.</exception>
    public static bool TryParseText(string? input, int maxLength, out string text)
    {
        // A non-positive limit is a programming mistake, not a user mistake,
        // so it is reported loudly instead of returning false.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        text = input?.Trim() ?? string.Empty;

        if (text.Length == 0 || text.Length > maxLength)
        {
            text = string.Empty;
            return false;
        }

        return true;
    }
}