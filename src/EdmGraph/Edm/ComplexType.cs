namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Complex Type
/// </summary>
public sealed class ComplexType(
    string Name,
    bool Abstract = false,
    bool OpenType = false) : EdmElement(Name), IEdmPropertyType
{
    private readonly List<Property> _properties = [];
    private readonly List<NavigationProperty> _navigationProperties = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Property> Properties => _properties;
    public IReadOnlyCollection<NavigationProperty> NavigationProperties => _navigationProperties;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    
    public Schema? ParentSchema { get; internal set; }

    public override string TargetPathSegment => $".{Name}";
    protected override EdmElement? Parent => ParentSchema;

    internal void AddProperty(Property property) 
    {
        property.ParentElement = this;
        _properties.Add(property);
    }
    
    internal void AddNavigationProperty(NavigationProperty navigationProperty) 
    {
        navigationProperty.ParentElement = this;
        _navigationProperties.Add(navigationProperty);
    }
    
    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _properties,
        .. _navigationProperties,
        .. _annotations
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Abstract", Abstract);
        yield return ("OpenType", OpenType);
    }

    public ComplexType? BaseType { get; internal set; } 
    public bool Abstract { get; } = Abstract;
    public bool OpenType { get; } = OpenType;
}

/// <summary>
/// Interface for elements that can be children of ComplexType
/// </summary>
public interface IComplexTypeChildElement
{
}
