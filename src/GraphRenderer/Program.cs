
using System.Text.Json;

var graph = new
{
    elements = new
    {
        nodes = new object[]
        {
            new { data = new { id = "a", label = "A" } },
            new { data = new { id = "b", label = "B" } },
            new { data = new { id = "c", label = "C" } },
            new { data = new { id = "d", label = "D"}, classes = new object[]{"foo", "bar"}},
            new { data = new { id = "e", label = "E" } }
        },
        edges = new object[]
        {
            new { data = new { source = "a", target = "b", label = "a to b" } },
            new { data = new { source = "a", target = "c", label = "a to c" } },
            new { data = new { source = "a", target = "d", label = "a to d" } },
            new { data = new { source = "d", target = "e", label = "d to e" } }
        },
    },
    style = new object[] // the stylesheet for the graph
    {
        new {
            selector = "node",
            style = new {
                shape= "round-rectangle",
                cornerRadius= "20px", // Adjusted corner radius for round corners
                color= "#000",
                backgroundColor= "#4a90e2",
                label= "data(label)",
                width= "5em",
                textValign= "center",
                textHalign= "center"
            }
        },
        new {
            selector = "edge",
            style = new {
                width= 3,
                lineColor= "#333",
                targetArrowShape= "triangle",
                targetArrowColor= "#333",
                label= "data(label)",
                fontSize= "12px",
                color= "#333",
                textBackgroundColor= "#fff",
                textBackgroundOpacity= 0.8,
                textBackgroundPadding= "3px",
                curveStyle= "bezier"
            }
        }
    },
    layout = new
    {
        // https://js.cytoscape.org/#layouts
        name = "breadthfirst", 
        // fit = true, // Fit the viewport to the graph

        // randomize = true, // Randomize the initial positions of nodes
        animate = true,
        animationDuration = 200, // The duration of the animation in milliseconds
        animationEasing = "ease-in-out", // The easing function for animate:'end'

        nodeDimensionsIncludeLabels = true, // Include labels in node dimensions
        spacingFactor = 1.75, // Spacing factor for the layout
        // idealEdgeLength = 100, // Ideal length of edges
        nodeOverlap = 10, // Minimum distance between nodes
        gravity = 0.90 // Gravity factor for the layout       
    }      
};
// https://js.cytoscape.org/#notation/elements-json

string json = JsonSerializer.Serialize(graph, new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
});
File.WriteAllText("graph.json", json);
