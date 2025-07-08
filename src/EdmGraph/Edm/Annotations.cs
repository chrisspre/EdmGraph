namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotations element for external targeting
/// </summary>
public sealed class Annotations(string Target, string? Qualifier = null) : IEdmElement, IEdmElementFactory<Annotations>
{
    public string Target { get; } = Target;
    public string? Qualifier { get; } = Qualifier;

    public Schema? ParentSchema { get; internal set; }

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> AnnotationCollection => _annotations;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    #region IEdmElement

    public LineInfo LineInfo { get; set; }

    string? IEdmElement.Name => "Annotations";
    IEdmElement? IEdmElement.Parent => ParentSchema;
    string IEdmElement.TargetPathSegment => $"/Annotations[@Target='{Target}']" +
        (Qualifier != null ? $"[@Qualifier='{Qualifier}']" : "");
    
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [.. _annotations];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Target", Target);
        if (Qualifier != null)
        {
            yield return ("Qualifier", Qualifier);
        }
    }

    #endregion

    public static Annotations Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var annotations = new Annotations(
           startElement.Get("Target"),
           startElement.Get("Qualifier", null));

        if (parent is Schema schema)
        {
            schema.AddAnnotations(annotations);
        }

        // TODO: Add deferred action for Target property resolution
        // The Target should resolve to the actual EdmElement being annotated
        // actions.Add(annotations, startElement.Get("Target"), static (element, path, resolver) => { 
        //     element.Target = resolver.Resolve<EdmElement>(path);
        // });

        return annotations;
    }
}
