using System.Diagnostics;
using System.Xml;


if (Debugger.IsAttached)
{
    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
}
Trace.Listeners.Add(new ConsoleTraceListener(true));

string[] fileNames = [
    @"data/microsoft-graph.csdl.xml",
    @"data/example89.csdl.xml",
    @"data/example90.csdl.xml",
];

foreach (var fileName in fileNames)
{
    var outputFileName = Path.ChangeExtension(fileName, Path.GetExtension(fileName) + ".txt");

    using var reader = new CsdlXmlReader(XmlReader.Create(fileName));
    using var output = File.CreateText(outputFileName);

    var sw = Stopwatch.StartNew();
    output.DisplayHierachy(reader);
    sw.Stop();
    Console.WriteLine("{0} {1}", sw.Elapsed, outputFileName);
}
