namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Type Definition
/// </summary>
public sealed class TypeDefinition(
    string Name,
    string UnderlyingType,
    int? MaxLength = null,
    bool Unicode = true,
    int? Precision = null,
    string? Scale = null,
    int? SRID = null) : EdmElement(Name)
{
    public string UnderlyingType { get; } = UnderlyingType;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public string? Scale { get; } = Scale;
    public int? SRID { get; } = SRID;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public Schema? ParentSchema { get; internal set; }

    public override string TargetPathSegment => $".{Name}";
    protected override EdmElement? Parent => ParentSchema;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("UnderlyingType", UnderlyingType);
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
