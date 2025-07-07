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
    string? DefaultValue = null
) 
: IEdmElement, IEntityTypeChildElement, IComplexTypeChildElement, IProperty, IEdmElementCreator<Property>
{
    public string Name { get; } = Name;
    public bool? Nullable { get; } = Nullable;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public Scale Scale { get; } = Scale;
    public int? SRID { get; } = SRID;
    public string? DefaultValue { get; } = DefaultValue;
    public bool IsCollection { get; internal set; }
    public IEdmPropertyType Type { get; internal set; } = null!;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public IEdmElement? ParentElement { get; internal set; }

    // IEdmElement implementation
    public string TargetPathSegment => $"/{Name}";
    public IEdmElement? Parent => ParentElement;
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
        yield return (nameof(Name), Name);
        if (Nullable ?? true) { yield return (nameof(Nullable), Nullable); }
        if (MaxLength.HasValue) { yield return (nameof(MaxLength), MaxLength.Value); }
        if (!Unicode) yield return (nameof(Unicode), Unicode);
        if (Precision.HasValue) { yield return (nameof(Precision), Precision.Value); }
        if (!Scale.Equals(default(Scale))) { yield return (nameof(Scale), Scale); }
        if (SRID.HasValue) { yield return (nameof(SRID), SRID.Value); }
        if (DefaultValue != null) { yield return (nameof(DefaultValue), DefaultValue); }
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static Property Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var property = new Property(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"),
            startElement.ParseNullable<int>("MaxLength"),
            startElement.Parse<bool>("Unicode", true),
            startElement.ParseNullable<int>("Precision"),
            startElement.Parse<Scale>("Scale"),
            startElement.ParseNullable<int>("SRID"),
            startElement.Get("DefaultValue", null));

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
        actions.Add(Priorities.Type, property, startElement.Get("Type"), SetTypeAction);
        return property;

        // static helper method for the deferred action to set the Type property of Property, NavigationProperty, Parameter, etc.
        static void SetTypeAction(Property element, string path, IElementResolver resolver)
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
