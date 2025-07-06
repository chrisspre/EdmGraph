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
    int? SRID = null) : EdmElement(Name)
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public Schema? ParentSchema { get; internal set; }

    public override string TargetPathSegment => $".{Name}";

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

    protected override EdmElement? Parent => ParentSchema;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [       .. _annotations   ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
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
}
