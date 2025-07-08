namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Structural Property
/// </summary>
public sealed class Property(string Name): EdmElement(Name), IEntityTypeChildElement, IComplexTypeChildElement, IProperty, IEdmElementFactory<Property>
{
    public bool Nullable { get; internal set; }
    public int? MaxLength { get; internal set; }
    public bool Unicode { get; internal set; }
    public int? Precision { get; internal set; }
    public Scale? Scale { get; internal set; }
    public int? SRID { get; internal set; }
    public string? DefaultValue { get; internal set; }

    public bool IsCollection { get; internal set; }

    public IEdmPropertyType Type { get; internal set; } = null!;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public IEdmElement ParentElement { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentElement;

    public override IReadOnlyCollection<IEdmElement> Children => _annotations.Cast<IEdmElement>().ToList();

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
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

    #endregion

    public static Property Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var property = new Property(startElement.Get("Name"));

        // Initialize nullable and facets from attributes
        property.Nullable = startElement.ParseNullable<bool>("Nullable") ?? false;
        property.MaxLength = startElement.ParseNullable<int>("MaxLength");
        property.Unicode = startElement.ParseNullable<bool>("Unicode") ?? false;
        property.Precision = startElement.ParseNullable<int>("Precision");
        property.Scale = startElement.ParseNullable<Scale>("Scale");
        property.SRID = startElement.ParseNullable<int>("SRID");
        property.DefaultValue = startElement.Get("DefaultValue");

        switch (parent)
        {
            case EntityType entityType:
                entityType.AddProperty(property);
                break;
            case ComplexType complexType:
                complexType.AddProperty(property);
                break;
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
