# Number Guessing Game and Grade Tracker (C#)

![CI](https://github.com/aimlin9/csharp-guessing-game-grade-tracker/actions/workflows/ci.yml/badge.svg)

Two C# console applications built with .NET 10, sharing one input-validation library:

- **Number Guessing Game:** guess a secret number with a limited number of tries, guided by higher/lower hints.
- **Grade Tracker:** store student scores in an array, see each student's letter grade, and view the class average, highest and lowest scores.

Both apps use a menu that repeats until the user quits, never crash on bad input, and are covered by 139 automated tests.

## Start here (for reviewers)

1. **Run the apps** (requires the .NET 10 SDK):
   ```bash
   dotnet run --project src/GuessingGame.Cli
   dotnet run --project src/GradeTracker.Cli
   ```
2. **Run the tests:** `dotnet test`
3. **Read the reasoning:** [docs/DESIGN.md](docs/DESIGN.md) explains the architecture and every key decision.
4. 4. **See coverage:** open the latest run in the [Actions tab](https://github.com/aimlin9/csharp-guessing-game-grade-tracker/actions). The coverage table is on the run's summary page, and the full HTML report can be downloaded as the `coverage-report` artifact. The latest run measured **98.5% line coverage** and **98.3% branch coverage**.

### Where each requirement is met

| Requirement | Where |
|---|---|
| Guessing game with limited tries | `GuessingRound` (`src/GuessingGame.Core`) |
| Grade tracker that stores scores in an array | `GradeBook` (`src/GradeTracker.Core`), a fixed-size `StudentRecord[]` |
| Average, highest and lowest score | `GradeBook.GetStatistics()`, a single loop over the array |
| A grade for each student | `GradeScale` and `StudentRecord.Grade` |
| Menu that repeats until the user quits | `GameApp.Run()` and `TrackerApp.Run()` |
| Code split into methods, not one long Main | `Program.cs` is two lines; each menu option is its own method in `GameApp` and `TrackerApp` |
| Handles wrong input | `ConsoleKit` (`InputParser`, `InputReader`), shared by both apps |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). The version is pinned in `global.json`, and any 10.0.x SDK works.
- Git

## Getting started

```bash
git clone https://github.com/aimlin9/csharp-guessing-game-grade-tracker.git
cd csharp-guessing-game-grade-tracker
dotnet build
dotnet test
```

In Visual Studio 2026, open `ConsoleApps.slnx`, right-click **GuessingGame.Cli** or **GradeTracker.Cli**, choose **Set as Startup Project**, and press **F5**.

## Number Guessing Game

| Difficulty | Range | Tries |
|---|---|---|
| Easy | 1 to 20 | 6 |
| Medium (default) | 1 to 50 | 7 |
| Hard | 1 to 100 | 7 |

Menu: **Play**, **Change difficulty**, **How to play**, **Quit**. After each round you can play again, and when you quit the game shows how many rounds you won.

```text
--- New round: Medium (1-50, 7 tries) ---
I'm thinking of a number between 1 and 50.
Your guess (7 tries left): 25
  Too high. Try a lower number.
Your guess (6 tries left): 12
  Too low. Try a higher number.
Your guess (5 tries left): 18
  Correct! You guessed it in 3 tries.
Play again? (y/n): n
```

## Grade Tracker

Menu: **Add students**, **View all students**, **View class statistics**, **Show grade scale**, **Quit**.

| Grade | Score |
|---|---|
| A | 70 to 100 |
| B | 60 to below 70 |
| C | 50 to below 60 |
| D | 40 to below 50 |
| F | 0 to below 40 |

```text
--- All students ---
  #   Name   Score  Grade
  -------------------------
  1   Ama       80  A
  2   Kofi    65.5  B
  3   Esi       92  A

  3 of 30 places used.

--- Class statistics ---
  Students: 3
  Average:  79.17 (A)
  Highest:  92 (Esi, A)
  Lowest:   65.5 (Kofi, B)
```

## Project structure

```text
csharp-guessing-game-grade-tracker/
├── src/
│   ├── ConsoleKit/           Shared input parsing and re-prompting
│   ├── GuessingGame.Core/    Game rules: Difficulty, GuessingRound, number generators
│   ├── GuessingGame.Cli/     Game menu (GameApp) and entry point
│   ├── GradeTracker.Core/    GradeBook (array), GradeScale, StudentRecord, statistics
│   └── GradeTracker.Cli/     Tracker menu (TrackerApp) and entry point
├── tests/
│   └── ConsoleApps.Tests/    Unit and end-to-end tests, one folder per library
├── docs/DESIGN.md            Architecture and engineering decisions
├── .github/workflows/ci.yml  Build, test and coverage on every push and PR
├── Directory.Build.props     Shared settings: warnings are treated as errors
└── global.json               Pins the .NET SDK version
```

## Testing

139 xUnit tests run with `dotnet test` and automatically on GitHub, with a coverage report on every run: **98.5% line coverage** (336 of 341 lines) and **98.3% branch coverage** (121 of 123 branches). The few uncovered lines are mainly the two-line `Program.cs` entry points, which the tests bypass by running `GameApp` and `TrackerApp` directly.

| Area | Tests | What they cover |
|---|---|---|
| ConsoleKit | 60 | Parsing every input type; re-prompting; range checks; closed input |
| Guessing game logic | 23 | Difficulty settings, random generator range, hints, tries, win and loss, reveal rules |
| Guessing game app | 10 | Full sessions: winning, losing, play again, difficulty change, garbage input |
| Grade tracker logic | 33 | Grade boundaries, student validation, array capacity, duplicates, statistics, ties |
| Grade tracker app | 13 | Full sessions: adding, viewing, statistics, full book, invalid input |

To produce the coverage report locally:

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html -assemblyfilters:"-ConsoleApps.Tests"
```

Then open `coverage-report/index.html`.

## Assumptions

**Guessing game**
- Secret numbers are whole numbers from 1 to the difficulty's maximum.
- A guess outside the range, or one that isn't a whole number, is rejected and does not use a try.
- Tries are set so every level can be won by halving the range each guess. Hard allows exactly enough.

**Grade tracker**
- Scores range from 0 to 100 and may have decimals (for example 87.5). The decimal separator is a dot.
- The grade scale follows common Ghanaian university grading (A from 70). Scores are not rounded before grading, so 69.99 is a B.
- Student names are required, up to 40 characters, and must be unique (ignoring letter case and surrounding spaces).
- The grade book holds 30 students, roughly one class.
- When two students tie for highest or lowest, the one entered first is shown.

## Known limitations

- Nothing is saved: students and game scores are lost when an app closes.
- Students cannot be edited or removed once added.
- The grade book's capacity is fixed at 30 and can only be changed in code.
- Statistics show one student for highest and lowest, even when several share the score.
- The game keeps a session win count only; there is no saved high-score table.
- Both apps are English only and console only.

## Development workflow

Work was done on short-lived branches and merged into `main` through pull requests:

- `chore/solution-scaffold`: solution structure and shared build settings
- `feat/console-kit`: shared input library
- `feat/guessing-game`: guessing game logic, app and tests
- `feat/grade-tracker`: grade tracker logic, app and tests
- `docs/readme-design-ci`: documentation, CI and coverage

Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/). Every pull request is built and tested by GitHub Actions before merging.

## Author

Gyimah Ramsey Opoku ([@aimlin9](https://github.com/aimlin9))