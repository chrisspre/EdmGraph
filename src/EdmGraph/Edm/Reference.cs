namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDMX Reference
/// </summary>
public sealed class Reference(
    string Uri) : EdmElement("Reference")
{
    public string Uri { get; } = Uri;
    
    public Edmx? ParentEdmx { get; internal set; }
    
    private readonly List<Include> _includes = [];
    private readonly List<IncludeAnnotations> _includeAnnotations = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Include> Includes => _includes;
    public IReadOnlyCollection<IncludeAnnotations> IncludeAnnotations => _includeAnnotations;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => $"/Reference[@Uri='{Uri}']";
    protected override EdmElement? Parent => ParentEdmx;

    internal void AddInclude(Include include)
    {
        _includes.Add(include);
        include.ParentReference = this;
    }
    
    internal void AddIncludeAnnotations(IncludeAnnotations includeAnnotations)
    {
        _includeAnnotations.Add(includeAnnotations);
        includeAnnotations.ParentReference = this;
    }
    
    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _includes,
        .. _includeAnnotations,
        .. _annotations
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Uri", Uri);
    }
}

/// <summary>
/// Represents an EDMX Include
/// </summary>
public sealed class Include(
    string Namespace,
    string? Alias = null) : EdmElement("Include")
{
    public string Namespace { get; } = Namespace;
    public string? Alias { get; } = Alias;
    
    public Reference? ParentReference { get; internal set; }
    
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => $"/Include[@Namespace='{Namespace}']";
    protected override EdmElement? Parent => ParentReference;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Namespace", Namespace);
        if (Alias != null)
        {
            yield return ("Alias", Alias);
        }
    }
}

/// <summary>
/// Represents an EDMX IncludeAnnotations
/// </summary>
public sealed class IncludeAnnotations(
    string TermNamespace,
    string? Qualifier = null,
    string? TargetNamespace = null) : EdmElement("IncludeAnnotations")
{
    public string TermNamespace { get; } = TermNamespace;
    public string? Qualifier { get; } = Qualifier;
    public string? TargetNamespace { get; } = TargetNamespace;
    
    public Reference? ParentReference { get; internal set; }
    
    public override string TargetPathSegment => $"/IncludeAnnotations[@TermNamespace='{TermNamespace}']";

    public override IReadOnlyCollection<EdmElement> Children => [];

    protected override EdmElement? Parent => ParentReference;

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {        yield return ("TermNamespace", TermNamespace);
        if (Qualifier != null)
        {
            yield return ("Qualifier", Qualifier);
        }
        if (TargetNamespace != null)
        {
            yield return ("TargetNamespace", TargetNamespace);
        }
    }
}