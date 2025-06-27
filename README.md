# EdmGraph

A .NET class library and tool to 
    - inspect and traverse an OData EDM model as an in-memory graph
    - load such an EDM model from CSDL XML

## Overview


- **EdmGraph**: A class library containing reusable components
- **EdmGraph.Console**: A console application that demonstrates the class library functionality

## Features


## Getting Started

### Prerequisites


### Running the demo console app

```bash

# Run the console application
dotnet run --project src/EdmGraph.Console
```

## Project Structure

```
EdmGraph/
├── src/
│   ├── EdmGraph/                    # Class library project
│   │   └── EdmGraph.csproj          # Class library project file
│   └── EdmGraph.Console/            # Console application project
│       ├── Program.cs               # Console application entry point
│       └── EdmGraph.Console.csproj  # Console project file
├── EdmGraph.sln                     # Solution file
├── .gitignore                       # Git ignore file
├── LICENSE                          # MIT License
└── README.md                        # This file
```

## Usage Examples

### Calculator

```csharp
using EdmGraph;

var graph = EdmGraph.FromXml("example_csdl.xml);

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
