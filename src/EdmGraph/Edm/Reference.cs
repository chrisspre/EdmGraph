
namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDMX Reference
/// </summary>
public sealed class Reference(string Uri) : IEdmElement, IEdmElementFactory<Reference>
{
    public string Uri { get; } = Uri;
    public Edmx? ParentEdmx { get; internal set; }

    private readonly List<Include> _includes = [];
    private readonly List<IncludeAnnotations> _includeAnnotations = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Include> Includes => _includes;
    public IReadOnlyCollection<IncludeAnnotations> IncludeAnnotations => _includeAnnotations;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    #region IEdmElement Implementation

    string? IEdmElement.Name => "Reference";
    string IEdmElement.TargetPathSegment => $"/Reference[@Uri='{Uri}']";
    IEdmElement? IEdmElement.Parent => ParentEdmx;
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [
        .. _includes,
        .. _includeAnnotations,
        .. _annotations
    ];
    LineInfo IEdmElement.LineInfo { get; set; }
    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Uri", Uri);
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

    public static Reference Create(CsdlToken.StartElement startElement, IEdmElement? parent, DeferredActionRegistry actions)
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
public sealed class Include(string Namespace, string? Alias = null) : IEdmElement, IEdmElementFactory<Include>
{
    public string Namespace { get; } = Namespace;

    public string? Alias { get; } = Alias;

    public Reference? ParentReference { get; internal set; }

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    #region IEdmElement Implementation

    string? IEdmElement.Name => "Include";
    string IEdmElement.TargetPathSegment => $"/Include[@Namespace='{Namespace}']";
    IEdmElement? IEdmElement.Parent => ParentReference;
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [.. _annotations];
    LineInfo IEdmElement.LineInfo { get; set; }
    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Namespace", Namespace);
        if (Alias != null)
        {
            yield return ("Alias", Alias);
        }
    }

    #endregion

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public static Include Create(CsdlToken.StartElement startElement, IEdmElement? parent, DeferredActionRegistry actions)
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
    : IEdmElement, IEdmElementFactory<IncludeAnnotations>
{
    public string TermNamespace { get; } = TermNamespace;
    public string? Qualifier { get; } = Qualifier;
    public string? TargetNamespace { get; } = TargetNamespace;

    public Reference? ParentReference { get; internal set; }


    #region IEdmElement Implementation

    string? IEdmElement.Name => "IncludeAnnotations";
    string IEdmElement.TargetPathSegment => $"/IncludeAnnotations[@TermNamespace='{TermNamespace}']";
    IEdmElement? IEdmElement.Parent => ParentReference;
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [];
    LineInfo IEdmElement.LineInfo { get; set;}
    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("TermNamespace", TermNamespace);
        if (Qualifier != null)
        {
            yield return ("Qualifier", Qualifier);
        }
        if (TargetNamespace != null)
        {
            yield return ("TargetNamespace", TargetNamespace);
        }
    }

    #endregion

    public static IncludeAnnotations Create(CsdlToken.StartElement startElement, IEdmElement? parent, DeferredActionRegistry actions)
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