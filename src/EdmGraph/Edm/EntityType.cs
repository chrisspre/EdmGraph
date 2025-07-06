namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Type
/// </summary>
public sealed class EntityType(
    string Name,
    bool Abstract = false,
    bool OpenType = false,
    bool HasStream = false) : EdmElement(Name)
{
    public bool Abstract { get; } = Abstract;
    public bool OpenType { get; } = OpenType;
    public bool HasStream { get; } = HasStream;

    public EntityType? BaseType { get; internal set; }

    private readonly List<Property> _properties = [];
    private readonly List<NavigationProperty> _navigationProperties = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Property> Properties => _properties;
    public IReadOnlyCollection<NavigationProperty> NavigationProperties => _navigationProperties;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    
    public Key? Key { get; private set; }
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
    internal void SetKey(Key key) 
    {
        key.ParentEntityType = this;
        Key = key;
    }


    public override IReadOnlyCollection<EdmElement> Children => [
        .. _properties,
        .. _navigationProperties,
        .. _annotations,
        .. (Key is not null ? [Key] : Enumerable.Empty<EdmElement>()) // Include Key if it exists
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        if (BaseType != null)
        {
            yield return ("BaseType", BaseType);
        }
        yield return ("Abstract", Abstract);
        yield return ("OpenType", OpenType);
        yield return ("HasStream", HasStream);
    }
}

/// <summary>
/// Interface for elements that can be children of EntityType
/// </summary>
public interface IEntityTypeChildElement
{
}

/// <summary>
/// Represents an EDM Key for an Entity Type
/// </summary>
public sealed class Key() : EdmElement("Key")
{
    private readonly List<PropertyRef> _propertyRefs = [];

    public IReadOnlyCollection<PropertyRef> PropertyRefs => _propertyRefs;
    public EntityType? ParentEntityType { get; internal set; }

    public override string TargetPathSegment => "/Key";
    protected override EdmElement? Parent => ParentEntityType;

    internal void AddPropertyRef(PropertyRef propertyRef) 
    {
        propertyRef.ParentKey = this;
        _propertyRefs.Add(propertyRef);
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _propertyRefs
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        // Key doesn't have additional basic properties beyond its name
        yield break;
    }
}

/// <summary>
/// Represents a Property Reference in a Key
/// </summary>
public sealed class PropertyRef(
    string Name,
    string? Alias = null) : EdmElement(Name)
{
    public Key? ParentKey { get; internal set; }

    public override string TargetPathSegment => null!; // PropertyRef does not have a target path segment on its own

    public string? Alias { get; } = Alias;


    public override IReadOnlyCollection<EdmElement> Children => [
        ];


    protected override EdmElement? Parent => ParentKey;

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        if (Alias != null)
        {
            yield return ("Alias", Alias);
        }
    }
}
