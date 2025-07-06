namespace EdmGraph.Edm;



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
    string? DefaultValue = null) : EdmElement(Name), IEntityTypeChildElement, IComplexTypeChildElement, IProperty, IEdmElementCreator
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public EdmElement? ParentElement { get; internal set; }

    public override string TargetPathSegment => $"/{Name}";

    protected override EdmElement? Parent => ParentElement;

    public IEdmPropertyType Type { get; internal set; } = null!;

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

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var property = new Property(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"),
            startElement.ParseNullable<int>("MaxLength"),
            startElement.Parse<bool>("Unicode", true),
            startElement.ParseNullable<int>("Precision"),
            startElement.Parse<Scale>("Scale"),
            startElement.ParseNullable<int>("SRID"),
            startElement.Get("DefaultValue", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityType entityType:
                    entityType.AddProperty(property);
                    break;
                case ComplexType complexType:
                    complexType.AddProperty(property);
                    break;
            }
        }
        actions.Add(Priorities.Type, property, startElement.Get("Type"), SetType);
        stack.Push(property);

        // static helper method for the deferred action to set the Type property of Property, NavigationProperty, Parameter, etc.
        static void SetType(Property element, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                element.IsCollection = true;
            }

            element.Type = resolver.Resolve<IEdmPropertyType>(path);
        }
    }
}
