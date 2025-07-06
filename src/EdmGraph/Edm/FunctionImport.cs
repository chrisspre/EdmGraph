namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Function Import
/// </summary>
public sealed class FunctionImport(
    string Name,
    string Function,
    string? EntitySet = null,
    bool IncludeInServiceDocument = false) : EdmElement(Name)
{
    public string Function { get; } = Function;
    public string? EntitySet { get; } = EntitySet;
    public bool IncludeInServiceDocument { get; } = IncludeInServiceDocument;
    
    public EntityContainer? ParentEntityContainer { get; internal set; }
    
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => $"/{Name}";
    protected override EdmElement? Parent => ParentEntityContainer;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Function", Function);
        if (EntitySet != null)
        {
            yield return ("EntitySet", EntitySet);
        }
        yield return ("IncludeInServiceDocument", IncludeInServiceDocument);
    }
}
