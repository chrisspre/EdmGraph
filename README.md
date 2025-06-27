# EdmGraph

A .NET class library and tool to 

- inspect and traverse an OData EDM model as an in-memory graph
- load such an EDM model from CSDL XML

## Overview


- **EdmGraph**: A class library that loads and manipulates OData EDM models as in-memory graphs
- **LabeledPropertyGraph.Net**: A library for working with labeled property graphs, is used by EdmGraph 
- **MermaidGen.Net**: A library to generate Mermaid diagrams from in-memory graphs. used by LabeledPropertyGraph 



### Running the demo console app

```bash

# Run the demo console application
dotnet run --project samples/EdmGraph.Console
```

## Project Structure

```
├── src/
│   ├── MermaidGen.Net/                      # Mermaid code generator for
│   ├── LabeledPropertyGraph.Net/           # Labeled Property Graph implementation
│   └── EdmGraph/                           # Main EdmGraph library
├── samples/
│   └── EdmGraph.Console/                   # Console application to demonstrate EdmGraph functionality    
├── tests/
│   ├── MermaidGen.Net.Tests/
│   ├── LabeledPropertyGraph.Net.Tests/
│   └── EdmGraph.Tests/
├── .gitignore
└── ...
```


```

## Usage Examples

### EdmGraph

```csharp
using EdmGraph;

var graph = EdmGraph.FromXml("example_csdl.xml");

// list all `EnumType`'s
foreach(var node in graph.Nodes.Where(node => node.Label == "EnumType")
{
    Console.WriteLine("{0} {1}", node, node.LineInfo);
}

EdmGraph.WriteToHtml(graph, "example_csdl.html");

```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please open an issue on the repository.
