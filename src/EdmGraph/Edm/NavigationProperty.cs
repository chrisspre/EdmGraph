using Serilog;

namespace EdmGraph.Edm;


/// <summary>
/// Represents an EDM Navigation Property
/// </summary>
public sealed class NavigationProperty(
    string Name,
    bool? Nullable = default,
    bool ContainsTarget = false) : IEdmElement, IEntityTypeChildElement, IComplexTypeChildElement, IProperty, IEdmElementCreator<NavigationProperty>
{
    private readonly List<ReferentialConstraint> _referentialConstraints = [];
    private readonly List<Annotation> _annotations = [];
    public IReadOnlyCollection<ReferentialConstraint> ReferentialConstraints => _referentialConstraints;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public OnDelete? OnDelete { get; private set; }
    
    public IEdmElement? ParentElement { get; internal set; }

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

    #region IEdmElement
    public LineInfo LineInfo { get; set; }
    string? IEdmElement.Name => Name;
    IEdmElement? IEdmElement.Parent => ParentElement;
    string IEdmElement.TargetPathSegment => $"/{Name}";
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [
        .. _annotations,
        .. _referentialConstraints,
        .. OnDelete is not null ? [OnDelete] : Enumerable.Empty<IEdmElement>()
    ];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("Nullable", Nullable);
        yield return ("ContainsTarget", ContainsTarget);
    }

    #endregion

    public static NavigationProperty Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var navigationProperty = new NavigationProperty(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"),
            startElement.Parse<bool>("ContainsTarget", false))       ;

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityType entityType:
                    entityType.AddNavigationProperty(navigationProperty);
                    break;
                case ComplexType complexType:
                    complexType.AddNavigationProperty(navigationProperty);
                    break;
            }
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
                    Log.Warning("NavigationProperty {element} has a Partner {path} but Type is not set yet.", element, path);
                    return;
                }
                element.Partner = resolver.Resolve<NavigationProperty>(element.Type, path);
                Log.Verbose("{element}'s Partner={path} resolved to {element.Partner}", element, path, element.Partner);
            });
        }

        stack.Push(navigationProperty);

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
                Log.Warning("Element '{path}' not found while trying setting the Type property of {element}", path, element);
            }
        }
    }
}

/// <summary>
/// Represents a Referential Constraint on a Navigation Property
/// </summary>
public sealed class ReferentialConstraint() : IEdmElement, IEdmElementCreator<ReferentialConstraint>
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public NavigationProperty? ParentNavigationProperty { get; internal set; }

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region IEdmElement

    string? IEdmElement.Name => "ReferentialConstraint";
    IEdmElement? IEdmElement.Parent => ParentNavigationProperty;
    string IEdmElement.TargetPathSegment => "/ReferentialConstraint";
    LineInfo IEdmElement.LineInfo  { get; set; } 
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [.. _annotations];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        // ReferentialConstraint doesn't have additional basic properties beyond its name
        yield break;
    }

    #endregion

    public static ReferentialConstraint Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var referentialConstraint = new ReferentialConstraint();

        if (stack.TryPeek(out var parent) && parent is NavigationProperty navigationProperty)
        {
            navigationProperty.AddReferentialConstraint(referentialConstraint);
        }

        stack.Push(referentialConstraint);
        return referentialConstraint;
    }
}

/// <summary>
/// Represents an On-Delete Action for a Navigation Property
/// </summary>
public sealed class OnDelete(string Action) : IEdmElement, IEdmElementCreator<OnDelete>
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public NavigationProperty? ParentNavigationProperty { get; internal set; }

    public string Action { get; } = Action;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region IEdmElement

    string? IEdmElement.Name => "OnDelete";
    IEdmElement? IEdmElement.Parent => ParentNavigationProperty;
    string IEdmElement.TargetPathSegment => "/OnDelete";
    LineInfo IEdmElement.LineInfo { get; set; } 
   
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [.. _annotations];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Action", Action);
    }

    #endregion

    public static OnDelete Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var onDelete = new OnDelete(startElement.Get("Action"));

        if (stack.TryPeek(out var parent) && parent is NavigationProperty navigationProperty)
        {
            navigationProperty.SetOnDelete(onDelete);
        }

        stack.Push(onDelete);
        return onDelete;
    }
}
