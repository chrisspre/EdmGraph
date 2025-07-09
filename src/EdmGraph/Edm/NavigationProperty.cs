using Serilog;

namespace EdmGraph.Edm;


/// <summary>
/// Represents an EDM Navigation Property
/// </summary>
public sealed class NavigationProperty(
    string Name,
    bool? Nullable = default,
    bool ContainsTarget = false) : EdmElement(Name), IEntityTypeChildElement, IComplexTypeChildElement, IProperty, IEdmElementFactory<NavigationProperty>
{
    private readonly List<ReferentialConstraint> _referentialConstraints = [];
    private readonly List<Annotation> _annotations = [];
    public IReadOnlyCollection<ReferentialConstraint> ReferentialConstraints => _referentialConstraints;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public OnDelete? OnDelete { get; private set; }
    
    public IEdmElement ParentElement { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddReferentialConstraint(ReferentialConstraint constraint) 
    {
        constraint.ParentNavigationProperty = this;
        _referentialConstraints.Add(constraint);
    }
    
    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);
    
    internal void SetOnDelete(OnDelete onDelete) 
    {
        onDelete.ParentNavigationProperty = this;
        OnDelete = onDelete;
    }

    public EntityType Type { get; internal set; } = null!; // Type will be set later via deferred action
    
    public bool IsCollection { get; internal set; }

    public bool? Nullable { get; } = Nullable;
    public NavigationProperty? Partner { get; internal set;} 
    public bool ContainsTarget { get; } = ContainsTarget;
    IEdmPropertyType IProperty.Type => Type;

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentElement;

    public override IReadOnlyCollection<IEdmElement> Children => OnDelete is not null 
        ? [.. _annotations, .. _referentialConstraints, OnDelete]
        : [.. _annotations, .. _referentialConstraints];

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("Nullable", Nullable);
        yield return ("ContainsTarget", ContainsTarget);
    }

    #endregion

    public static NavigationProperty Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var navigationProperty = new NavigationProperty(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"),
            startElement.Parse<bool>("ContainsTarget", false));

        switch (parent)
        {
            case EntityType entityType:
                entityType.AddNavigationProperty(navigationProperty);
                break;
            case ComplexType complexType:
                complexType.AddNavigationProperty(navigationProperty);
                break;
        }

        // Add deferred action for Type property resolution
        actions.Add(Priorities.Type, navigationProperty, startElement.Get("Type"), SetTypeAction);

        // Add deferred action for Partner property resolution if Partner is not null
        var partner = startElement.Get("Partner", null);
        if (partner is not null)
        {
            actions.Add(Priorities.Partner, navigationProperty, partner, static (element, path, resolver) =>
            {
                if (element.Type is null)
                {
                    Log.Warning("NavigationProperty {element} has a Partner \"{path}\" but it's Type is not set.", element, path);
                    return;
                }
                element.Partner = resolver.Resolve<NavigationProperty>(element.Type, path);
                Log.Verbose("{element}'s Partner={path} resolved to {element.Partner}", element, path, element.Partner);
            });
        }

        return navigationProperty;

        static void SetTypeAction(NavigationProperty element, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                element.IsCollection = true;
            }

            element.Type = resolver.Resolve<EntityType>(path);
            if (element.Type is null)
            {
                Log.Warning("'{path}' not found while trying to set the Type of {element}", path, element);
            }
        }
    }
}

/// <summary>
/// Represents a Referential Constraint on a Navigation Property
/// </summary>
public sealed class ReferentialConstraint(string Property, string ReferencedProperty) : EdmElement("ReferentialConstraint"), IEdmElementFactory<ReferentialConstraint>
{
    public string Property { get; } = Property;
    public string ReferencedProperty { get; } = ReferencedProperty;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public NavigationProperty ParentNavigationProperty { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentNavigationProperty;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => "/ReferentialConstraint";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Property), Property);
        yield return (nameof(ReferencedProperty), ReferencedProperty);
    }

    #endregion

    public static ReferentialConstraint Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var referentialConstraint = new ReferentialConstraint(
            startElement.Get("Property"),
            startElement.Get("ReferencedProperty"));

        if (parent is NavigationProperty navigationProperty)
        {
            navigationProperty.AddReferentialConstraint(referentialConstraint);
        }

        return referentialConstraint;
    }
}

/// <summary>
/// Represents an On-Delete Action for a Navigation Property
/// </summary>
public sealed class OnDelete(string Action) : EdmElement("OnDelete"), IEdmElementFactory<OnDelete>
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public NavigationProperty ParentNavigationProperty { get; internal set; } = default!; // Set in ModelBuilder/Create

    public string Action { get; } = Action;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentNavigationProperty;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => "/OnDelete";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Action", Action);
    }

    #endregion

    public static OnDelete Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var onDelete = new OnDelete(startElement.Get("Action"));

        if (parent is NavigationProperty navigationProperty)
        {
            navigationProperty.SetOnDelete(onDelete);
        }

        return onDelete;
    }
}
