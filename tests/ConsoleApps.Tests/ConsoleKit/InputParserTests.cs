using ConsoleKit;

namespace ConsoleApps.Tests;

/// <summary>
/// Unit tests for <see cref="InputParser"/>.
/// These protect the "never crash on bad input" requirement for both apps.
/// </summary>
public class InputParserTests
{
    // ---------- Decimals ----------

    public static TheoryData<string, decimal> ValidDecimals => new()
    {
        { "87.5", 87.5m },
        { "100", 100m },
        { "0", 0m },
        { "-3.25", -3.25m },
        { "  42  ", 42m },     // stray spaces are forgiven
        { "+7", 7m },
    };

    [Theory]
    [MemberData(nameof(ValidDecimals))]
    public void TryParseDecimal_ValidInput_ReturnsTrueAndValue(string input, decimal expected)
    {
        var success = InputParser.TryParseDecimal(input, out var value);

        Assert.True(success);
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(null)]                                // input stream closed
    [InlineData("")]                                  // user just pressed Enter
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("12abc")]
    [InlineData("1,5")]                               // comma rejected on purpose
    [InlineData("1.2.3")]
    [InlineData("--5")]
    [InlineData("1e5")]                               // scientific notation not supported
    [InlineData("99999999999999999999999999999999")]  // too large for decimal
    public void TryParseDecimal_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(InputParser.TryParseDecimal(input, out _));
    }


    // ---------- Integers ----------

    [Theory]
    [InlineData("42", 42)]
    [InlineData("-3", -3)]
    [InlineData(" 7 ", 7)]
    [InlineData("+5", 5)]
    [InlineData("0", 0)]
    public void TryParseInteger_ValidInput_ReturnsTrueAndValue(string input, int expected)
    {
        var success = InputParser.TryParseInteger(input, out var value);

        Assert.True(success);
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("2.5")]           // guesses must be whole numbers
    [InlineData("1,000")]
    [InlineData("99999999999")]   // too large for int
    [InlineData("--1")]
    public void TryParseInteger_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(InputParser.TryParseInteger(input, out _));
    }


    // ---------- Yes / no ----------

    [Theory]
    [InlineData("y", true)]
    [InlineData("Y", true)]
    [InlineData("yes", true)]
    [InlineData("YES", true)]
    [InlineData(" Yes ", true)]
    [InlineData("n", false)]
    [InlineData("N", false)]
    [InlineData("no", false)]
    [InlineData("No", false)]
    public void TryParseYesNo_RecognisedAnswer_ReturnsTrueAndAnswer(string input, bool expected)
    {
        var success = InputParser.TryParseYesNo(input, out var answer);

        Assert.True(success);
        Assert.Equal(expected, answer);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("maybe")]
    [InlineData("ye")]
    [InlineData("1")]
    public void TryParseYesNo_UnrecognisedAnswer_ReturnsFalse(string? input)
    {
        Assert.False(InputParser.TryParseYesNo(input, out _));
    }


    // ---------- Text ----------

    [Theory]
    [InlineData("Ama", "Ama")]
    [InlineData("  Kofi Mensah  ", "Kofi Mensah")]   // trimmed, inner space kept
    public void TryParseText_ValidInput_ReturnsTrimmedText(string input, string expected)
    {
        var success = InputParser.TryParseText(input, 50, out var text);

        Assert.True(success);
        Assert.Equal(expected, text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]   // only spaces counts as empty
    public void TryParseText_EmptyInput_ReturnsFalse(string? input)
    {
        Assert.False(InputParser.TryParseText(input, 50, out var text));
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryParseText_ExactlyMaxLength_IsAccepted()
    {
        var input = new string('a', 50);

        Assert.True(InputParser.TryParseText(input, 50, out _));
    }

    [Fact]
    public void TryParseText_LongerThanMaxLength_ReturnsFalse()
    {
        var input = new string('a', 51);

        Assert.False(InputParser.TryParseText(input, 50, out _));
    }

    [Fact]
    public void TryParseText_NonPositiveMaxLength_ThrowsBecauseItIsAProgrammingError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => InputParser.TryParseText("Ama", 0, out _));
    }
}