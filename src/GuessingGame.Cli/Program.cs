using GuessingGame.Cli;
using GuessingGame.Core;

// Entry point. Kept deliberately tiny: all behaviour lives in GameApp, which receives
// its input, output and number generator from here. Tests pass scripted text and a
// fixed secret number instead, so the exact same code can be played automatically.
var app = new GameApp(Console.In, Console.Out, new RandomSecretNumberGenerator());

return app.Run();