namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotations element for external targeting
/// </summary>
public sealed class Annotations(string Target, string? Qualifier = null) : EdmElement("Annotations"), IEdmElementCreator
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

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var annotations = new Annotations(
           startElement.Get("Target"),
           startElement.Get("Qualifier", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddAnnotations(annotations);
        }

        // TODO: Add deferred action for Target property resolution
        // The Target should resolve to the actual EdmElement being annotated
        // actions.Add(annotations, startElement.Get("Target"), static (element, path, resolver) => { 
        //     element.Target = resolver.Resolve<EdmElement>(path);
        // });

        stack.Push(annotations);
    }
}
