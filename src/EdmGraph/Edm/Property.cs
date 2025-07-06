namespace EdmGraph.Edm;


public interface IPropertyType : IEdmElement { }

/// <summary>
/// Represents an EDM Structural Property
/// </summary>
public sealed class Property(
    string Name,
    bool? Nullable,
    int? MaxLength = null,
    bool Unicode = true,
    int? Precision = null,
    Scale Scale = default,
    int? SRID = null,
    string? DefaultValue = null) : EdmElement(Name), IEntityTypeChildElement, IComplexTypeChildElement
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public EdmElement? ParentElement { get; internal set; }

    public override string TargetPathSegment => $"/{Name}";

    protected override EdmElement? Parent => ParentElement;

    public IPropertyType Type { get; internal set; } = null!;

    public bool? Nullable { get; } = Nullable;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public Scale Scale { get; } = Scale;
    public int? SRID { get; } = SRID;
    public string? DefaultValue { get; } = DefaultValue;
    public bool IsCollection { get; internal set; }

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Name), Name);
        if (Nullable ?? true) { yield return (nameof(Nullable), Nullable); }
        if (MaxLength.HasValue) { yield return (nameof(MaxLength), MaxLength.Value); }
        if (!Unicode) yield return (nameof(Unicode), Unicode);
        if (Precision.HasValue) { yield return (nameof(Precision), Precision.Value); }
        if (!Scale.Equals(default(Scale))) { yield return (nameof(Scale), Scale); }
        if (SRID.HasValue) { yield return (nameof(SRID), SRID.Value); }
        if (DefaultValue != null) { yield return (nameof(DefaultValue), DefaultValue); }
    }
}

