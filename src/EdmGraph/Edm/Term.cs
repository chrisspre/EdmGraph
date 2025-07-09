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
    int? SRID = null) : EdmElement(Name), IEdmElementFactory<Term>
{
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
    public Schema ParentSchema { get; internal set; } = default!; // Set in ModelBuilder/Create

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentSchema;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => $".{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Name), Name);
        yield return (nameof(Type), Type);
        if (BaseTerm != null)
        {
            yield return (nameof(BaseTerm), BaseTerm);
        }
        if (AppliesTo != null)
        {
            yield return (nameof(AppliesTo), AppliesTo);
        }
        if (DefaultValue != null)
        {
            yield return (nameof(DefaultValue), DefaultValue);
        }
        yield return (nameof(Nullable), Nullable);
        if (MaxLength.HasValue)
        {
            yield return (nameof(MaxLength), MaxLength.Value);
        }
        yield return (nameof(Unicode), Unicode);
        if (Precision.HasValue)
        {
            yield return (nameof(Precision), Precision.Value);
        }
        if (Scale != null)
        {
            yield return (nameof(Scale), Scale);
        }
        if (SRID.HasValue)
        {
            yield return (nameof(SRID), SRID.Value);
        }
    }

    #endregion

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public static Term Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var term = new Term(
            startElement.Get("Name"),
            startElement.Get("Type"),
            startElement.Get("BaseTerm", null),
            startElement.Get("AppliesTo", null),
            startElement.Get("DefaultValue", null),
            startElement.Parse<bool>("Nullable", true))
        {
            LineInfo = startElement.LineInfo
        };

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
