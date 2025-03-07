using System.Diagnostics;
using System.Text.RegularExpressions;
using Humanizer;
using omm.generator;

namespace omm;

internal class Program
{
    private static void Main(string[] args)
    {
        if (Debugger.IsAttached)
        {
            Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
        }
        else
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }


        foreach (var isSyntax in new[] { true, false })
        {
            var path = isSyntax ? "../EdmGraph/Syn" : "../EdmGraph/Sem";
            EnsureDirectoryExists(path);

            using var writer = new StreamWriter(Path.Combine(path, "Model.cs"));
            Generator.Generate(OData401.Model, writer, isSyntax);
        }
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

