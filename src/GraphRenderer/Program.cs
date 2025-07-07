
using System.Text.Json;
using EdmGraph;
using EdmGraph.Edm;


internal class Program
{
    private static void Main(string[] args)
    {
        string path = @"D:\microsoft\EdmGraph\data\reporting-line.xml";

        // Log.Information("Processing file: {path}", path);
        var sw = Stopwatch.StartNew();
        var model = ModelBuilder.FromFile(path);
        sw.Stop();
        // Log.Information("Loaded {FileName} in {Elapsed}", Path.GetFileNameWithoutExtension(path), sw.Elapsed);

        using var stream = File.Create("graph.json");
        using var xmlWriter = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true, IndentSize = 4 });
        xmlWriter.WriteStartObject();
        xmlWriter.WriteStartObject("elements");
        xmlWriter.WriteStartArray("nodes");
        var idCounter = 0;
        string idGenerator() => $"n{++idCounter}";
        var index = new Dictionary<IEdmElement, string>(ReferenceEqualityComparer.Instance);
        foreach (var element in model.DescendantsAndSelf())
        {
            xmlWriter.WriteStartObject();
            xmlWriter.WritePropertyName("data");
            xmlWriter.WriteStartObject();
            var id = idGenerator();
            xmlWriter.WriteString("id", id);
            index[element] = id;
            var label = $"{element.GetType().Name}{(string.IsNullOrWhiteSpace(element.Name) ? "" : $": {element.Name}")}";
            xmlWriter.WriteString("label", label);
            xmlWriter.WriteEndObject(); // data
            xmlWriter.WriteEndObject(); // node
                                        // Log.Information("{Name} {Type} {LineInfo}", edmElement.Name, edmElement.GetType().Name, edmElement.LineInfo);
        }
        xmlWriter.WriteEndArray(); // nodes

        xmlWriter.WriteStartArray("edges");
        foreach (var element in model.DescendantsAndSelf())
        {
            var targetId = index[element];
            if (element.Parent is not null)
            {
                var sourceId = index[element.Parent];

                xmlWriter.WriteStartObject();
                xmlWriter.WritePropertyName("data");
                xmlWriter.WriteStartObject();
                xmlWriter.WriteString("source", sourceId);
                xmlWriter.WriteString("target", targetId);
                // xmlWriter.WriteString("label", $"child");
                xmlWriter.WriteEndObject(); // data
                xmlWriter.WriteEndObject(); // edge
            }
        }
        xmlWriter.WriteEndArray(); // edges

        xmlWriter.WriteEndObject(); // elements
        xmlWriter.WritePropertyName("layout"); xmlWriter.WriteRawValue(LAYOUT);
        xmlWriter.WritePropertyName("style"); xmlWriter.WriteRawValue(STYLE);
        xmlWriter.WriteEndObject(); // root object
    }


    private const string LAYOUT = """
        {
            "name": "breadthfirst",
            "fit": true,
            "animate": true,
            "animationDuration": 200,
            "animationEasing": "ease-in-out",
            "nodeDimensionsIncludeLabels": true,
            "nodeOverlap": 10,
            "gravity": 0.30
        }
    """;

    private const string STYLE = """
        [
            {
                "selector": "node",
                "style": {
                    "shape": "round-rectangle",
                    "corner-radius": "20px",
                    "color": "#000",
                    "background-color": "#ddf",
                    "label": "data(label)",
                    "width": "label",
                    "height": "label",
                    "text-wrap": "wrap",
                    "text-max-width": "200px",
                    "padding": "15px", 
                    "text-valign": "center",
                    "text-halign": "center"
                }
            },
            {
                "selector": "edge",
                "style": {
                    "width": 3,
                    "line-color": "#333",
                    "target-arrow-shape": "triangle",
                    "target-arrow-color": "#333",
                    "label": "data(label)",
                    "font-size": 24,
                    "color": "#333",
                    "text-background-color": "#fff",
                    "text-background-opacity": 0.8,
                    "text-background-padding": 3,
                    "curve-style": "bezier"
                }
            }
        ]
    """;

    static Program()
    {
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


//     var graph = new
// {
//     elements = new
//     {
//         nodes = new object[]
//         {
//             new { data = new { id = "a", label = "A" } },
//             new { data = new { id = "b", label = "B" } },
//             new { data = new { id = "c", label = "C" } },
//             new { data = new { id = "d", label = "D"}, classes = new object[]{"foo", "bar"}},
//             new { data = new { id = "e", label = "E" } }
//         },
//         edges = new object[]
//         {
//             new { data = new { source = "a", target = "b", label = "a to b" } },
//             new { data = new { source = "a", target = "c", label = "a to c" } },
//             new { data = new { source = "a", target = "d", label = "a to d" } },
//             new { data = new { source = "d", target = "e", label = "d to e" } }
//         },
//     },
//     style = new object[] // the stylesheet for the graph
//     {
//         new {
//             selector = "node",
//             style = new {
//                 shape= "round-rectangle",
//                 cornerRadius= "20px", // Adjusted corner radius for round corners
//                 color= "#000",
//                 backgroundColor= "#4a90e2",
//                 label= "data(label)",
//                 width= "5em",
//                 textValign= "center",
//                 textHalign= "center"
//             }
//         },
//         new {
//             selector = "edge",
//             style = new {
//                 width= 3,
//                 lineColor= "#333",
//                 targetArrowShape= "triangle",
//                 targetArrowColor= "#333",
//                 label= "data(label)",
//                 fontSize= "12px",
//                 color= "#333",
//                 textBackgroundColor= "#fff",
//                 textBackgroundOpacity= 0.8,
//                 textBackgroundPadding= "3px",
//                 curveStyle= "bezier"
//             }
//         }
//     },
//     layout = new
//     {
//         // https://js.cytoscape.org/#layouts
//         name = "breadthfirst",
//         // fit = true, // Fit the viewport to the graph

//         // randomize = true, // Randomize the initial positions of nodes
//         animate = true,
//         animationDuration = 200, // The duration of the animation in milliseconds
//         animationEasing = "ease-in-out", // The easing function for animate:'end'

//         nodeDimensionsIncludeLabels = true, // Include labels in node dimensions
//         spacingFactor = 1.75, // Spacing factor for the layout
//         // idealEdgeLength = 100, // Ideal length of edges
//         nodeOverlap = 10, // Minimum distance between nodes
//         gravity = 0.90 // Gravity factor for the layout
//     }
// };
// // https://js.cytoscape.org/#notation/elements-json

// string json = JsonSerializer.Serialize(graph, new JsonSerializerOptions
// {
//     WriteIndented = true,
//     PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
// });
// File.WriteAllText("graph.json", json);
