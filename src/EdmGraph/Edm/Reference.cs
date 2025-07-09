
namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDMX Reference
/// </summary>
public sealed class Reference(string Uri) : EdmElement("Reference"), IEdmElementFactory<Reference>
{
    public string Uri { get; } = Uri;
    public Edmx ParentEdmx { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<Include> _includes = [];
    private readonly List<IncludeAnnotations> _includeAnnotations = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Include> Includes => _includes;
    public IReadOnlyCollection<IncludeAnnotations> IncludeAnnotations => _includeAnnotations;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEdmx;

    public override IReadOnlyCollection<IEdmElement> Children => [
        .. _includes,
        .. _includeAnnotations,
        .. _annotations
    ];

    public override string TargetPathSegment => $"/Reference[@Uri='{Uri}']";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Uri), Uri);
    }

    #endregion

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

    public static Reference Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var reference = new Reference(startElement.Get("Uri"));

        if (parent is Edmx edmx)
        {
            edmx.AddReference(reference);
        }

        return reference;
    }
}

/// <summary>
/// Represents an EDMX Include
/// </summary>
public sealed class Include(string Namespace, string? Alias = null) : EdmElement("Include"), IEdmElementFactory<Include>
{
    public string Namespace { get; } = Namespace;

    public string? Alias { get; } = Alias;

    public Reference ParentReference { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentReference;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => $"/Include[@Namespace='{Namespace}']";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Namespace), Namespace);
        if (Alias != null)
        {
            yield return (nameof(Alias), Alias);
        }
    }

    #endregion

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public static Include Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var include = new Include(
           startElement.Get("Namespace"),
           startElement.Get("Alias", null));

        if (parent is Reference reference)
        {
            reference.AddInclude(include);
        }

        return include;
    }
}

/// <summary>
/// Represents an EDMX IncludeAnnotations
/// </summary>
public sealed class IncludeAnnotations(string TermNamespace, string? Qualifier = null, string? TargetNamespace = null) 
    : EdmElement("IncludeAnnotations"), IEdmElementFactory<IncludeAnnotations>
{
    public string TermNamespace { get; } = TermNamespace;
    public string? Qualifier { get; } = Qualifier;
    public string? TargetNamespace { get; } = TargetNamespace;

    public Reference ParentReference { get; internal set; } = default!; // Set in ModelBuilder/Create

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentReference;

    public override IReadOnlyCollection<IEdmElement> Children => [];

    public override string TargetPathSegment => $"/IncludeAnnotations[@TermNamespace='{TermNamespace}']";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(TermNamespace), TermNamespace);
        if (Qualifier != null)
        {
            yield return (nameof(Qualifier), Qualifier);
        }
        if (TargetNamespace != null)
        {
            yield return (nameof(TargetNamespace), TargetNamespace);
        }
    }

    #endregion

    public static IncludeAnnotations Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var includeAnnotations = new IncludeAnnotations(
            startElement.Get("TermNamespace"),
            startElement.Get("Qualifier", null),
            startElement.Get("TargetNamespace", null));

        if (parent is Reference reference)
        {
            reference.AddIncludeAnnotations(includeAnnotations);
        }

        return includeAnnotations;
    }
}