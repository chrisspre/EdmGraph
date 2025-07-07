namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Type
/// </summary>
public sealed class EntityType(
    string Name,
    bool Abstract = false,
    bool OpenType = false,
    bool HasStream = false) : IEdmElement, IEdmPropertyType, IEdmElementCreator<EntityType>
{
    public string Name { get; } = Name;
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

    // IEdmElement implementation
    public string TargetPathSegment => $".{Name}";
    
    public IEdmElement? Parent => ParentSchema;

    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

    public IReadOnlyCollection<IEdmElement> Children 
    {
        get
        {
            var children = new List<IEdmElement>();
            children.AddRange(_properties.Cast<IEdmElement>());
            // TODO: Add navigation properties back after they're refactored
            // children.AddRange(_navigationProperties.Cast<IEdmElement>());
            children.AddRange(_annotations.Cast<IEdmElement>());
            if (Key != null)
            {
                children.Add(Key);
            }
            return children;
        }
    }

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

    public IEnumerable<(string, object?)> GetBasicProperties()
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

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static EntityType Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var entityType = new EntityType(
            startElement.Get("Name"),
            startElement.Parse<bool>("Abstract"),
            startElement.Parse<bool>("OpenType"),
            startElement.Parse<bool>("HasStream"));

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Schema schema:
                    schema.AddEntityType(entityType);
                    break;
            }
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
public sealed class Key() : IEdmElement, IEdmElementCreator<Key>
{
    public string Name => "Key";
    
    private readonly List<PropertyRef> _propertyRefs = [];

    public IReadOnlyCollection<PropertyRef> PropertyRefs => _propertyRefs;
    public EntityType? ParentEntityType { get; internal set; }

    // IEdmElement implementation
    public string TargetPathSegment => "/Key";
    public IEdmElement? Parent => ParentEntityType;
    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

    public IReadOnlyCollection<IEdmElement> Children => _propertyRefs.Cast<IEdmElement>().ToList();

    internal void AddPropertyRef(PropertyRef propertyRef)
    {
        propertyRef.ParentKey = this;
        _propertyRefs.Add(propertyRef);
    }

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        // Key doesn't have additional basic properties beyond its name
        yield break;
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static Key Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var key = new Key();

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityType entityType:
                    entityType.SetKey(key);
                    break;
            }
        }

        return key;
    }
}



/// <summary>
/// Represents a Property Reference in a Key
/// </summary>
public sealed class PropertyRef(string? Alias = null) : IEdmElement, IEdmElementCreator<PropertyRef>
{
    public string Name => "PropertyRef";
    public string? Alias { get; } = Alias;
    public Key? ParentKey { get; internal set; }
    public PropertyPath PropertyPath { get; internal set; } = null!; // Initialized later in ModelBuilder;

    // IEdmElement implementation
    public string TargetPathSegment => null!; // PropertyRef does not have a target path segment on its own
    public IEdmElement? Parent => ParentKey;
    public IReadOnlyCollection<IEdmElement> Children => [];
    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        if (Alias != null)
        {
            yield return ("Alias", Alias);
        }
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static PropertyRef Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var propertyRef = new PropertyRef(startElement.Get("Alias"));

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Key key:
                    key.AddPropertyRef(propertyRef);
                    break;
            }
        }

        //  Add deferred action for property reference resolution
        actions.Add(Priorities.PropertyRef, propertyRef, startElement.Get("Name"), static (element, path, resolver) =>
        {
            // PrepertyRef `Name` are resolved starting with the EntityType of the Key or the PropertyRef
            var name = resolver.ResolvePropertyPath(element.ParentKey!.ParentEntityType!, path);

            element.PropertyPath = name;
        });

        return propertyRef;
    }
}
