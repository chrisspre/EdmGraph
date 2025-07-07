namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Complex Type
/// </summary>
public sealed class ComplexType(
    string Name,
    string? BaseType = null,
    bool IsAbstract = false,
    bool OpenType = false) : IEdmElement, IEdmPropertyType, IEdmElementCreator<ComplexType>
{
    public string? BaseType { get; } = BaseType;
    public bool IsAbstract { get; } = IsAbstract;
    public bool OpenType { get; } = OpenType;

    public Schema? ParentSchema { get; internal set; }

    private readonly List<Property> _properties = [];
    private readonly List<NavigationProperty> _navigationProperties = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Property> Properties => _properties;
    public IReadOnlyCollection<NavigationProperty> NavigationProperties => _navigationProperties;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

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

    internal void AddAnnotation(Annotation annotation)
    {
        annotation.ParentElement = this;
        _annotations.Add(annotation);
    }

    #region IEdmElement
    public LineInfo LineInfo { get;  set; }
    string? IEdmElement.Name => Name;
    IEdmElement? IEdmElement.Parent => ParentSchema;
    string IEdmElement.TargetPathSegment => $".{Name}";
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [
        .. _properties,
        .. _navigationProperties,
        .. _annotations
    ];
    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Name", ((IEdmElement)this).TargetPath);
        if (BaseType != null)
        {
            yield return ("BaseType", BaseType);
        }
        yield return ("IsAbstract", IsAbstract);
        yield return ("OpenType", OpenType);
    }

    #endregion

    public static ComplexType Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var complexType = new ComplexType(
            startElement.Get("Name"),
            startElement.Get("BaseType", null),
            startElement.Parse<bool>("Abstract", false),
            startElement.Parse<bool>("OpenType", false));

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddComplexType(complexType);
        }

        // TODO: Add deferred action for BaseType property resolution
        // if (complexType.BaseType != null)
        // {
        //     actions.Add(complexType, complexType.BaseType, static (element, path, resolver) => { 
        //         element.BaseType = resolver.Resolve<ComplexType>(path);
        //     });
        // }

        stack.Push(complexType);
        return complexType;
    }
}

/// <summary>
/// Marker interface for elements that can be children of ComplexType
/// </summary>
public interface IComplexTypeChildElement
{
}
