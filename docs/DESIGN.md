# Design and Engineering Decisions

This document explains how the two applications are structured and why each key decision was made.

## 1. Architecture

```text
          GuessingGame.Cli                    GradeTracker.Cli
         (GameApp, Program)                (TrackerApp, Program)
           │            │                    │            │
           ▼            └──────┐     ┌───────┘            ▼
   GuessingGame.Core           ▼     ▼              GradeTracker.Core
   (Difficulty,              ConsoleKit             (GradeBook, GradeScale,
    GuessingRound,       (InputParser,               StudentRecord,
    generators)           InputReader)               GradeStatistics)

                    ConsoleApps.Tests tests all five projects
```

Dependencies point one way. Each Core library knows nothing about the console, so its rules are tested directly and could be reused behind a web or desktop interface. The two Core libraries do not depend on each other or on ConsoleKit.

### Why a shared ConsoleKit library?
Both apps need the same thing: read a number or answer, reject bad input with a clear hint, and ask again. Writing that once means it is tested once (60 tests), and both apps behave consistently. It is an upgraded version of the parser from Assignment 1, extended with whole numbers, yes/no answers and text.

### Code split into methods
`Program.cs` in each app is two lines: create the app object and run it. Each app class then has one method per menu option (`PlayRound`, `ChooseDifficulty`, `AddStudents`, `ShowStatistics` and so on) plus small helpers for output and formatting. The menu loop in `Run()` only reads a choice and dispatches to the right method.

## 2. Key decisions

### The secret number is injected, not hard-wired to Random
`GuessingRound` asks an `ISecretNumberGenerator` for its secret. The app passes `RandomSecretNumberGenerator`; tests pass `FixedSecretNumberGenerator`, which always returns a known number. Without this, testing "too low, too high, correct" would depend on luck. The random generator uses `Random.Shared`, which avoids the classic bug of creating `new Random()` repeatedly and getting repeated sequences. The round also checks the generator's result, so a faulty generator fails loudly instead of creating an unwinnable game.

### Tries per difficulty are based on binary search
Halving the remaining range each guess finds any number in at most ceil(log2(n + 1)) guesses: 5 for 1-20, 6 for 1-50 and 7 for 1-100. Easy and Medium give one spare try; Hard gives exactly enough, so it rewards strategy without being unfair.

### Invalid guesses never cost a try
The console only accepts guesses inside the range, and `GuessingRound` validates before counting a try as a safety net. A typo should not lose the game.

### The secret is hidden until the round ends
`RevealSecretNumber()` throws while the round is in progress, so no code path can leak the answer early.

### A real fixed-size array for grades
The brief asks for scores stored in an array. `GradeBook` holds a `StudentRecord[]` of fixed capacity plus a count of filled slots. When it is full, it says so clearly. A `List<T>` would grow silently and hide the array behaviour the exercise is about. `GetStudents()` returns a copy of only the filled part, so callers can neither change the grade book nor see empty slots.

### Statistics in a single explicit loop
Average, highest and lowest come from one pass over the array using a plain `for` loop rather than LINQ, which shows the iteration the assignment is about and avoids walking the data three times. Comparisons are strictly greater or smaller, so on a tie the first student entered is kept, which is deterministic and tested. An empty grade book throws instead of dividing by zero, and the app checks for that case first and explains it to the user.

### One grade table, used everywhere
`GradeScale.Bands` is an ordered table (A at 70 down to F at 0). Grading uses it, and the "Show grade scale" screen prints it, so what users see can never disagree with what the program does. Changing the policy means editing one table. Scores are not rounded before grading, so 69.99 is a B.

### Records are valid from the moment they exist
`StudentRecord` validates its name and score in the constructor and cannot change afterwards, so any record that exists is valid. The grade is calculated from the score rather than stored, so the two can never disagree.

### Validate at the edge, with Core as a safety net
The apps catch problems where the user can fix them: a duplicate name or out-of-range score is re-asked immediately. Core still validates everything, so the rules hold even if another interface forgets.

### Format checks and range checks are separate
`InputParser` only checks the format (is this a number?). `InputReader` then checks the range (is it between 1 and 50?). This gives the user a precise hint for each kind of mistake. User mistakes never throw; programming mistakes, such as a range where min is greater than max, throw immediately.

### Closed input ends the app cleanly
If the input stream closes (Ctrl+Z, or piped input running out), `Console.ReadLine()` returns null forever. `InputReader` turns this into an `InputEndedException`, and both apps catch it and exit with code 0 instead of looping forever.

### Console access is injected
Both apps take a `TextReader` and `TextWriter`. The real apps pass the console; tests pass in-memory text and play whole sessions automatically.

### Build settings that keep quality high
- `Directory.Build.props` sets `TreatWarningsAsErrors`, so possible null references, analyzer warnings and similar issues must be fixed before the build passes.
- `global.json` pins the .NET 10 SDK (any 10.0.x), so every machine and CI build with the same toolchain.

### Naming
The game engine is `GuessingRound`, not `GuessingGame`, because a class sharing its namespace's name (`GuessingGame.Core.GuessingGame`) causes confusing compiler errors in C#.

## 3. Testing strategy

139 tests across three layers:

1. **Shared input (60):** every input type, valid and invalid, boundaries, re-prompting and closed input.
2. **Core logic (56):** game rules with a fixed secret number, grade boundaries tested at each band's minimum and just below it, array capacity, duplicates, defensive copies and statistics including ties.
3. **End to end (23):** scripted sessions through `GameApp` and `TrackerApp`, checking the printed output for wins, losses, errors, full grade books and garbage input at every prompt.

Randomness is tested by its properties rather than exact values: 1,000 draws must all fall in range and cover every value.

GitHub Actions builds the solution, runs every test and publishes a code coverage report on each push and pull request. Coverage is measured on application code only; the test project is excluded.

## 4. What I would change with more time

- **Persistence:** save students to a JSON file so the grade book survives restarts.
- **Edit and remove students**, which would also need the array to shift entries down to close gaps.
- **Show every tied student** for highest and lowest, not just the first.
- **A saved high-score table** for the game, keyed by difficulty.
- **Interfaces for Core services** (for example `IGradeBook`), so the console layer could be tested with stand-ins as well as end to end.