namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotations element for external targeting
/// </summary>
public sealed class Annotations(
    string Target,
    string? Qualifier = null) : EdmElement("Annotations")
{
    public string Target { get; } = Target;
    public string? Qualifier { get; } = Qualifier;
    
    public Schema? ParentSchema { get; internal set; }
    
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> AnnotationCollection => _annotations;

    public override string TargetPathSegment => $"/Annotations[@Target='{Target}']" + 
        (Qualifier != null ? $"[@Qualifier='{Qualifier}']" : "");
    
    protected override EdmElement? Parent => ParentSchema;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }
    
    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Target", Target);
        if (Qualifier != null)
        {
            yield return ("Qualifier", Qualifier);
        }
    }
}
