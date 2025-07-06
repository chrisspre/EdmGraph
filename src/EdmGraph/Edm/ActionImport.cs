namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Action Import
/// </summary>
public sealed class ActionImport(
    string Name,
    string Action,
    string? EntitySet = null) : EdmElement(Name)
{
    public EntityContainer? ParentEntityContainer { get; internal set; }

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => $"/{Name}";
    protected override EdmElement? Parent => ParentEntityContainer;

    public string Action { get; } = Action;
    public string? EntitySet { get; } = EntitySet;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations,
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Action", Action);
        if (EntitySet != null)
        {
            yield return ("EntitySet", EntitySet);
        }
    }
}
