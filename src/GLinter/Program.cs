namespace EdmGraph.Console;

using EdmGraph;
using EdmGraph.Edm;
using Serilog;
using System; // allows to write Console.WriteLine

class Program
{

    static void Main()
    {
        string[] paths = [
            // @"..\..\data\example89.xml",
            @"..\..\data\microsoft-graph.xml",
            // @"..\..\data\reporting-line.xml",
        ];


        foreach (var path in paths)
        {
            Log.Information("Processing file: {path}", path);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var model = ModelBuilder.FromFile(path);
            sw.Stop();
            Log.Information("Loaded {FileName} in {Elapsed}", Path.GetFileNameWithoutExtension(path), sw.Elapsed);


            Log.Information("linting {Name}", Path.GetFileNameWithoutExtension(path));
            foreach (var (id, error, element) in Linter.RunRules(model))
            {
                Log.Warning("{id} {error} {path}{lineInfo}", id, error, path, element.LineInfo);
            }



            using (var logFile = new StreamWriter("logs/tree.log"))
            {
                model.WriteAsciiTree(logFile);
                Log.Information("Tree written to {Name}", ((FileStream)logFile.BaseStream).Name);
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            var filePath = $"logs/{fileName}.html";
            MermaidGraph.WriteGraph(model, filePath, fileName);
            Log.Information("Graph written to {Name}", filePath);
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
