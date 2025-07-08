namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Term
/// </summary>
public sealed class Term(
    string Name,
    string Type,
    string? BaseTerm = null,
    string? AppliesTo = null,
    string? DefaultValue = null,
    bool Nullable = true,
    int? MaxLength = null,
    bool Unicode = true,
    int? Precision = null,
    string? Scale = null,
    int? SRID = null) : IEdmElement, IEdmElementFactory<Term>
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public string? BaseTerm { get; } = BaseTerm;
    public string? AppliesTo { get; } = AppliesTo;
    public string? DefaultValue { get; } = DefaultValue;
    public bool Nullable { get; } = Nullable;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public string? Scale { get; } = Scale;
    public int? SRID { get; } = SRID;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public Schema? ParentSchema { get; internal set; }

    // IEdmElement implementation
    public string TargetPathSegment => $".{Name}";
    public IEdmElement? Parent => ParentSchema;
    public IReadOnlyCollection<IEdmElement> Children => _annotations.Cast<IEdmElement>().ToList();
    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Type", Type);
        if (BaseTerm != null)
        {
            yield return ("BaseTerm", BaseTerm);
        }
        if (AppliesTo != null)
        {
            yield return ("AppliesTo", AppliesTo);
        }
        if (DefaultValue != null)
        {
            yield return ("DefaultValue", DefaultValue);
        }
        yield return ("Nullable", Nullable);
        if (MaxLength.HasValue)
        {
            yield return ("MaxLength", MaxLength.Value);
        }
        yield return ("Unicode", Unicode);
        if (Precision.HasValue)
        {
            yield return ("Precision", Precision.Value);
        }
        if (Scale != null)
        {
            yield return ("Scale", Scale);
        }
        if (SRID.HasValue)
        {
            yield return ("SRID", SRID.Value);
        }
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static Term Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var term = new Term(
            startElement.Get("Name"),
            startElement.Get("Type"),
            startElement.Get("BaseTerm", null),
            startElement.Get("AppliesTo", null),
            startElement.Get("DefaultValue", null),
            startElement.Parse<bool>("Nullable", true));

        if (parent is Schema schema)
        {
            schema.AddTerm(term);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(term, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<ITermType>(path);
        // });

        // TODO: Add deferred action for BaseTerm property resolution if BaseTerm is not null
        var baseTerm = startElement.Get("BaseTerm", null);
        if (baseTerm is not null)
        {
            // actions.Add(term, baseTerm, static (element, path, resolver) => { 
            //     element.BaseTerm = resolver.Resolve<Term>(path);
            // });
        }

        return term;
    }
}
