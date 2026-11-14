using System.Globalization;

using ConsoleKit;

using GradeTracker.Core;

namespace GradeTracker.Cli;

/// <summary>
/// The grade tracker's console app: a menu that repeats until the user quits.
/// </summary>
/// <remarks>
/// How responsibilities are split:
/// - GradeBook (Core) stores students in an array and calculates statistics.
/// - GradeScale (Core) decides letter grades.
/// - InputReader (ConsoleKit) guarantees we only receive well-formed input.
/// - TrackerApp (this class) runs the menu and formats everything for people to read.
///
/// Validation happens at the edge first: duplicate names and out-of-range scores
/// are caught here and re-asked, so the user fixes one value instead of starting
/// over. Core still validates everything as a safety net.
/// </remarks>
public sealed class TrackerApp
{
    private const int MenuAdd = 1;
    private const int MenuView = 2;
    private const int MenuStatistics = 3;
    private const int MenuGradeScale = 4;
    private const int MenuQuit = 5;

    private readonly TextWriter _output;
    private readonly InputReader _reader;
    private readonly GradeBook _book;


    /// <param name="capacity">How many students the grade book can hold. Tests use a small number.</param>
    public TrackerApp(TextReader input, TextWriter output, int capacity = GradeBook.DefaultCapacity)
    {
        _output = output;
        _reader = new InputReader(input, output);
        _book = new GradeBook(capacity);
    }


    /// <summary>
    /// Runs the menu until the user quits or the input stream ends.
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
                var choice = _reader.ReadInteger("Choose an option (1-5): ", 1, 5);
                _output.WriteLine();

                switch (choice)
                {
                    case MenuAdd:
                        AddStudents();
                        break;

                    case MenuView:
                        ShowAllStudents();
                        break;

                    case MenuStatistics:
                        ShowStatistics();
                        break;

                    case MenuGradeScale:
                        ShowGradeScale();
                        break;

                    case MenuQuit:
                        _output.WriteLine("Goodbye!");
                        return 0;
                }

                _output.WriteLine();
            }
        }
        catch (InputEndedException)
        {
            _output.WriteLine();
            _output.WriteLine("Input ended. Goodbye!");
            return 0;
        }
    }


    // ---------- Adding students ----------

    private void AddStudents()
    {
        WriteHeading("Add students");

        while (true)
        {
            if (_book.IsFull)
            {
                WriteFullMessage();
                return;
            }

            var name = ReadNewStudentName();
            var score = _reader.ReadDecimal("Score (0-100): ", GradeScale.MinScore, GradeScale.MaxScore);

            var student = _book.Add(name, score);
            _output.WriteLine($"  Added {student.Name}: {FormatScore(student.Score)} ({student.Grade})");

            // Don't offer another entry if there is no room for one.
            if (_book.IsFull)
            {
                WriteFullMessage();
                return;
            }

            if (!_reader.ReadYesNo("Add another student? (y/n): "))
            {
                return;
            }
        }
    }

    // Keeps asking until the name is not already in the grade book.
    private string ReadNewStudentName()
    {
        while (true)
        {
            var name = _reader.ReadText("Student name: ", StudentRecord.MaxNameLength);

            if (!_book.Contains(name))
            {
                return name;
            }

            _output.WriteLine($"  {name} is already in the grade book. Please use a different name.");
        }
    }

    private void WriteFullMessage() =>
        _output.WriteLine($"  The grade book is full ({_book.Capacity} students). No more can be added.");


    // ---------- Viewing ----------

    private void ShowAllStudents()
    {
        WriteHeading("All students");

        if (_book.IsEmpty)
        {
            _output.WriteLine("  No students yet. Choose option 1 to add some.");
            return;
        }

        var students = _book.GetStudents();

        // Make the name column as wide as the longest name, so the columns line up.
        var nameWidth = "Name".Length;
        foreach (var student in students)
        {
            nameWidth = Math.Max(nameWidth, student.Name.Length);
        }

        _output.WriteLine($"  {"#",-3} {"Name".PadRight(nameWidth)}  {"Score",6}  Grade");
        _output.WriteLine("  " + new string('-', 3 + 1 + nameWidth + 2 + 6 + 2 + 5));

        for (var i = 0; i < students.Length; i++)
        {
            var student = students[i];
            _output.WriteLine(
                $"  {i + 1,-3} {student.Name.PadRight(nameWidth)}  {FormatScore(student.Score),6}  {student.Grade}");
        }

        _output.WriteLine();
        _output.WriteLine($"  {students.Length} of {_book.Capacity} places used.");
    }

    private void ShowStatistics()
    {
        WriteHeading("Class statistics");

        // Statistics need at least one score; check first instead of letting Core throw.
        if (_book.IsEmpty)
        {
            _output.WriteLine("  No scores yet. Add students first to see statistics.");
            return;
        }

        var stats = _book.GetStatistics();

        _output.WriteLine($"  Students: {stats.Count}");
        _output.WriteLine($"  Average:  {FormatScore(stats.Average)} ({stats.AverageGrade})");
        _output.WriteLine($"  Highest:  {FormatScore(stats.Highest.Score)} ({stats.Highest.Name}, {stats.Highest.Grade})");
        _output.WriteLine($"  Lowest:   {FormatScore(stats.Lowest.Score)} ({stats.Lowest.Name}, {stats.Lowest.Grade})");
    }

    private void ShowGradeScale()
    {
        WriteHeading("Grade scale");

        // Printed straight from GradeScale.Bands, so what users see is
        // always exactly what the program uses.
        var bands = GradeScale.Bands;

        for (var i = 0; i < bands.Count; i++)
        {
            var upper = i == 0
                ? FormatScore(GradeScale.MaxScore)
                : $"below {FormatScore(bands[i - 1].MinScore)}";

            _output.WriteLine($"  {bands[i].Grade}  {FormatScore(bands[i].MinScore)} to {upper}");
        }
    }


    // ---------- Output helpers ----------

    private void WriteBanner()
    {
        _output.WriteLine("==============================");
        _output.WriteLine("    Student Grade Tracker");
        _output.WriteLine("==============================");
        _output.WriteLine();
    }

    private void WriteMainMenu()
    {
        _output.WriteLine("Main menu");
        _output.WriteLine("  1. Add students");
        _output.WriteLine("  2. View all students");
        _output.WriteLine("  3. View class statistics");
        _output.WriteLine("  4. Show grade scale");
        _output.WriteLine("  5. Quit");
    }

    private void WriteHeading(string title) => _output.WriteLine($"--- {title} ---");

    /// <summary>
    /// Rounds to 2 decimal places for display and drops trailing zeros (87.50 shows as 87.5).
    /// Calculations always use full precision; rounding happens only here.
    /// </summary>
    private static string FormatScore(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);
}