using ConsoleApp;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Start");

    var pathToTexts = Path.Combine(Directory.GetCurrentDirectory(), "Texts", args[0]);
    var pathToStopWords = Path.Combine(Directory.GetCurrentDirectory(), "StopWords", args[1]);
    var pathToLemmitization = Path.Combine(Directory.GetCurrentDirectory(), "Lemmitization", args[2]);
    
    await ApplicationRunner.RunAsync(pathToTexts, pathToStopWords, pathToLemmitization);
}
finally
{
    Log.CloseAndFlush();
}