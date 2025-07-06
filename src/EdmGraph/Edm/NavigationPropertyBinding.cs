namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Navigation Property Binding
/// </summary>
public sealed class NavigationPropertyBinding(
    string Path,
    string Target) : EdmElement("NavigationPropertyBinding")
{
    public string Path { get; } = Path;
    public string Target { get; } = Target;
    
    public EdmElement? ParentElement { get; internal set; }
    
    public override string TargetPathSegment => null!; // NavigationPropertyBinding does not have a target path segment on its own
    
    protected override EdmElement? Parent => ParentElement;

    public override IReadOnlyCollection<EdmElement> Children => [
        // NavigationPropertyBinding does not have children
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Path", Path);
        yield return ("Target", Target);
    }
}
