namespace EdmGraph.Edm;

public interface IEdmElement
{
    string Name { get; }
   
    string? TargetPath { get; }

    EdmElement? Parent { get; }
    
    IReadOnlyCollection<EdmElement> Children { get; }

    IEnumerable<(string, object?)> GetBasicProperties();
}
