namespace GradeTracker.Core;

/// <summary>
/// Stores students and their scores in a fixed-size array, and calculates statistics.
/// </summary>
/// <remarks>
/// Why a fixed-size array?
/// The brief asks for scores stored in an array. A plain array has a fixed length,
/// so the grade book tracks how many slots are filled (_count) and refuses new
/// students once it is full, with a clear message. A List would grow by itself,
/// but would hide the array behaviour the exercise is about.
///
/// Only the first _count slots hold real students; the rest are empty and are
/// never read.
/// </remarks>
public sealed class GradeBook
{
    /// <summary>Default size, roughly one class.</summary>
    public const int DefaultCapacity = 30;

    private readonly StudentRecord[] _students;
    private int _count;


    /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is less than 1.</exception>
    public GradeBook(int capacity = DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

        _students = new StudentRecord[capacity];
    }


    public int Capacity => _students.Length;
    public int Count => _count;
    public bool IsEmpty => _count == 0;
    public bool IsFull => _count == _students.Length;


    /// <summary>
    /// Adds a student and returns the stored record, including the grade.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the grade book is full.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the name is invalid or already used, or the score is out of range.
    /// </exception>
    public StudentRecord Add(string name, decimal score)
    {
        if (IsFull)
        {
            throw new InvalidOperationException($"The grade book is full ({Capacity} students).");
        }

        // Creating the record validates the name and score first.
        var student = new StudentRecord(name, score);

        if (Contains(student.Name))
        {
            throw new ArgumentException($"A student named {student.Name} is already in the grade book.");
        }

        // Fill the next empty slot, then move the "filled" marker along by one.
        _students[_count] = student;
        _count++;

        return student;
    }


    /// <summary>Checks whether a student with this name exists, ignoring letter case and spaces.</summary>
    public bool Contains(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var wanted = name.Trim();

        for (var i = 0; i < _count; i++)
        {
            if (string.Equals(_students[i].Name, wanted, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Returns the students in the order they were added.
    /// </summary>
    /// <remarks>
    /// Returns a copy of just the filled part of the array, so callers can never
    /// change the grade book's contents or see the empty slots.
    /// </remarks>
    public StudentRecord[] GetStudents() => _students[.._count];


    /// <summary>
    /// Calculates the average, highest and lowest scores in one pass through the array.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no students have been added.</exception>
    public GradeStatistics GetStatistics()
    {
        // An average of nothing is undefined, so refuse rather than divide by zero.
        if (IsEmpty)
        {
            throw new InvalidOperationException("No scores have been entered yet.");
        }

        var total = 0m;
        var highest = _students[0];
        var lowest = _students[0];

        for (var i = 0; i < _count; i++)
        {
            var student = _students[i];

            total += student.Score;

            // Strictly greater / smaller, so on a tie the first student entered is kept.
            if (student.Score > highest.Score)
            {
                highest = student;
            }

            if (student.Score < lowest.Score)
            {
                lowest = student;
            }
        }

        return new GradeStatistics(_count, total / _count, highest, lowest);
    }
}