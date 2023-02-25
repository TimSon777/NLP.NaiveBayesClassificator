using ConsoleApp;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Start");

var path = Path.Combine(Directory.GetCurrentDirectory(), "Texts", args[0]);

await ApplicationRunner.RunAsync(path);

Log.CloseAndFlush();
