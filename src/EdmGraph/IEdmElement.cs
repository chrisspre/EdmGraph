namespace EdmGraph;

public interface IEdmElement
{
    // the majority of EDM elements have a name. In the few cases they don't, the name is null
    string? Name { get; }

    LineInfo LineInfo { get; set; }

    // the parent element is null for the root element (DataService), otherwise it is the parent element
    IEdmElement? Parent { get; }
    IReadOnlyCollection<IEdmElement> Children { get; }
    IEnumerable<(string, object?)> GetBasicProperties();
    string TargetPathSegment { get; }
    string? TargetPath => Parent is null ? null : Parent.TargetPath + TargetPathSegment;
}

