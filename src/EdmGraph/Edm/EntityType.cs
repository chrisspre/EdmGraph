namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Type
/// </summary>
public sealed class EntityType(
    string Name,
    bool Abstract = false,
    bool OpenType = false,
    bool HasStream = false) : EdmElement(Name), IEdmPropertyType, IEdmElementFactory<EntityType>
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
    public Schema ParentSchema { get; internal set; } = default!; // Set in ModelBuilder/Create
    
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

    #region EdmElement abstract member implementation
    
    public override IEdmElement Parent => ParentSchema;

    public override IReadOnlyCollection<IEdmElement> Children => Key != null 
        ? [ .. _properties, .. _navigationProperties, .. _annotations, Key ]
        : [ .. _properties, .. _navigationProperties, .. _annotations ];
        

    public override string TargetPathSegment => $".{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        if (BaseType != null)
        {
            yield return ("BaseType", BaseType);
        }
        yield return ("Abstract", Abstract);
        yield return ("OpenType", OpenType);
        yield return ("HasStream", HasStream);
    }

    #endregion

    public static EntityType Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var entityType = new EntityType(
            startElement.Get("Name"),
            startElement.Parse<bool>("Abstract"),
            startElement.Parse<bool>("OpenType"),
            startElement.Parse<bool>("HasStream"));

        if (parent is Schema schema)
        {
            schema.AddEntityType(entityType);
        }

        var baseType = startElement.Get("BaseType");
        if (baseType is not null && !string.IsNullOrEmpty(baseType))
        {
            actions.Add(2, entityType, startElement.Get("BaseType"), static (element, path, resolver) =>
            {
                element.BaseType = resolver.Resolve<EntityType>(path);
            });
        }
        
        return entityType;
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
public sealed class Key() : EdmElement("Key"), IEdmElementFactory<Key>
{
    private readonly List<PropertyRef> _propertyRefs = [];

    public IReadOnlyCollection<PropertyRef> PropertyRefs => _propertyRefs;
    public EntityType ParentEntityType { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddPropertyRef(PropertyRef propertyRef)
    {
        propertyRef.ParentKey = this;
        _propertyRefs.Add(propertyRef);
    }

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEntityType;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _propertyRefs];

    public override string TargetPathSegment => "/Key";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        // Key doesn't have additional basic properties beyond its name
        yield break;
    }

    #endregion

    public static Key Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var key = new Key();

        if (parent is EntityType entityType)
        {
            entityType.SetKey(key);
        }

        return key;
    }
}



/// <summary>
/// Represents a Property Reference in a Key
/// </summary>
public sealed class PropertyRef(string? Alias = null) : EdmElement("PropertyRef"), IEdmElementFactory<PropertyRef>
{
    public string? Alias { get; } = Alias;
    public Key ParentKey { get; internal set; } = default!; // Set in ModelBuilder/Create
    public PropertyPath PropertyPath { get; internal set; } = null!; // Initialized later in ModelBuilder

    #region EdmElement abstract member implementation

    public new string Name => PropertyPath.Last().Name;

    public override IEdmElement Parent => ParentKey;

    public override IReadOnlyCollection<IEdmElement> Children => [];

    public override string TargetPathSegment => null!; // PropertyRef does not have a target path segment

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        if (Alias != null)
        {
            yield return ("Alias", Alias);
        }
    }

    #endregion

    public static PropertyRef Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var propertyRef = new PropertyRef(startElement.Get("Alias"));

        if (parent is Key key)
        {
            key.AddPropertyRef(propertyRef);
        }

        //  Add deferred action for property reference resolution
        actions.Add(Priorities.PropertyRef, propertyRef, startElement.Get("Name"), static (element, path, resolver) =>
        {
            // PropertyRef `Name` are resolved starting with the EntityType of the Key or the PropertyRef
            var name = resolver.ResolvePropertyPath(element.ParentKey.ParentEntityType, path);

            element.PropertyPath = name;
        });

        return propertyRef;
    }
}
