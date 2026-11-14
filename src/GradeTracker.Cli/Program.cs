using GradeTracker.Cli;

// Entry point. Kept deliberately tiny: all behaviour lives in TrackerApp, which
// receives its input and output from here. Tests pass scripted text instead.
var app = new TrackerApp(Console.In, Console.Out);

return app.Run();