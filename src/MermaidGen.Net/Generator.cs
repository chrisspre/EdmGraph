namespace MermaidGen.Net;

public static class Generator
{

    public static void WriteGraphAsHtml(
        string outputFilePath,
        List<(string Id, string Label, string? Category)> nodes,
        List<(string Source, string Target, string Type)> links,
        string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath, nameof(outputFilePath));

        using var outputWriter = new StreamWriter(outputFilePath);
        WriteGraphAsHtml(title, outputWriter, nodes, links);
    }

    public static void WriteGraphAsHtml(string title, TextWriter outputWriter,
        List<(string Id, string Label, string? Category)> nodes,
        List<(string Source, string Target, string Type)> links)
    {
        // Create template placeholder replacements 
        var template = new Template
        {
            ["Title"] = title,
            ["MermaidMarkup"] = (ReplacementValue)(writer => WriteMermaidMarkup(writer, nodes, links)),
            ["MermaidConfig"] = "%%{init: {'theme': 'default', 'themeVariables': {'primaryColor': '#ffcc00'}}}%%"
        };

        template.Expand("MermaidGen.Net.resources.mermaid_zoom_pan.html", outputWriter, debug: false);
    }


    private static void WriteMermaidMarkup(TextWriter writer,
        List<(string Id, string Label, string? Category)> nodes,
        List<(string Source, string Target, string Type)> links)
    {

        // Render the Mermaid diagram
        writer.WriteLine("graph TD");

        // Group nodes by category and render subgraphs
        foreach (var category in nodes.GroupBy(g => g.Category))
        {
            if (category.Key is not null) { writer.WriteLine($"subgraph {category.Key}"); }
            foreach (var (id, label, _) in category)
            {
                writer.WriteLine($"    {id}[{label}]");
            }
            if (category.Key is not null) { writer.WriteLine("end"); }
            writer.WriteLine();
        }

        // Render relationships
        writer.WriteLine("%% parent-child relationships");
        foreach (var (source, target, type) in links.Where(r => r.Type == "parent"))
        {
            writer.WriteLine($"    {source} --> {target}");
        }
        writer.WriteLine();

        writer.WriteLine("%% other relationships");
        foreach (var (source, target, type) in links.Where(r => r.Type != "parent"))
        {
            writer.WriteLine($"{source} -.->|{type}| {target}");
        }
        writer.WriteLine();

        // Add styling
        writer.WriteLine("%% Styling");
        writer.WriteLine("classDef default fill:#fff,stroke:#000,stroke-width:3px,color:#000");
        writer.WriteLine("classDef subgraf fill:#eee,stroke:#eee,stroke-width:1px,color:#000");

        // Apply styles to subgraphs
        writer.WriteLine($"class {string.Join(",", nodes.Select(n => n.Category).Distinct())} subgraf");

    }
}
