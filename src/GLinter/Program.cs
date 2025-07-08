namespace GLinter;

using EdmGraph;
using Serilog;

class Program
{

    static void Main()
    {
        string[] paths = [
            @"..\..\data\example89.xml",
            // @"..\..\data\microsoft-graph.xml",
            @"..\..\data\reporting-line.xml",
        ];

        foreach (var path in paths)
        {
            var load = Operation.Begin($"LoadModel {path}");
            var model = ModelBuilder.FromFile(path);
            load.Complete();

            var linting = Operation.Begin($"linting {Path.GetFileNameWithoutExtension(path)}");
            foreach (var (id, error, element) in Linter.RunRules(model))
            {
                Log.Warning("{id} {error} {path}{lineInfo}", id, error, path, element.LineInfo);
            }
            linting.Complete();


            // using (var logFile = new StreamWriter("logs/tree.log"))
            // {
            //     model.WriteAsciiTree(logFile);
            //     Log.Information("Tree written to {Name}", ((FileStream)logFile.BaseStream).Name);
            // }

        }
    }

    static Program()
    {
        // Configure Serilog for logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
            .CreateLogger();
        Log.Information("EdmGraph Console Application started.");


        if (Debugger.IsAttached)
        {
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\..");
        }
        if (!Directory.Exists("logs"))
        {
            Directory.CreateDirectory("logs");
        }
    }
}
