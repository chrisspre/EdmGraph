using Serilog;

namespace EdmGraph.Edm;


/// <summary>
/// Represents an EDM Navigation Property
/// </summary>
public sealed class NavigationProperty(
    string Name,
    bool? Nullable = default,
    bool ContainsTarget = false) : EdmElement(Name), IEntityTypeChildElement, IComplexTypeChildElement, IProperty, IEdmElementCreator
{
    private readonly List<ReferentialConstraint> _referentialConstraints = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<ReferentialConstraint> ReferentialConstraints => _referentialConstraints;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public OnDelete? OnDelete { get; private set; }
    
    public EdmElement? ParentElement { get; internal set; }

    public override string TargetPathSegment => $"/{Name}";
    
    protected override EdmElement? Parent => ParentElement;

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


    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations,
        .. _referentialConstraints,
        .. OnDelete is not null ? [OnDelete] : Enumerable.Empty<EdmElement>()
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("Nullable", Nullable);
        yield return ("ContainsTarget", ContainsTarget);
    }

    public EntityType Type { get; internal set; } = null!; // Type will be set later via deferred action
    
    public bool IsCollection { get; internal set; }

    public bool? Nullable { get; } = Nullable;
    public NavigationProperty? Partner { get; internal set;} 
    public bool ContainsTarget { get; } = ContainsTarget;

    IEdmPropertyType IProperty.Type => Type;

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var navigationProperty = new NavigationProperty(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"))
        {
            LineInfo = startElement.LineInfo
        };

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
        actions.Add(Priorities.Type, navigationProperty, startElement.Get("Type"), SetType);

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

        static void SetType(NavigationProperty element, string path, IElementResolver resolver)
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
public sealed class ReferentialConstraint() : EdmElement("ReferentialConstraint"), IEdmElementCreator
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public NavigationProperty? ParentNavigationProperty { get; internal set; }

    public override string TargetPathSegment => null!; // ReferentialConstraint does not have a target path segment on its own
    protected override EdmElement? Parent => ParentNavigationProperty;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        // ReferentialConstraint doesn't have additional basic properties beyond its name
        yield break;
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var referentialConstraint = new ReferentialConstraint()
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is NavigationProperty navigationProperty)
        {
            navigationProperty.AddReferentialConstraint(referentialConstraint);
        }

        stack.Push(referentialConstraint);
    }
}

/// <summary>
/// Represents an On-Delete Action for a Navigation Property
/// </summary>
public sealed class OnDelete(    string Action) : EdmElement("OnDelete"), IEdmElementCreator
{
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public NavigationProperty? ParentNavigationProperty { get; internal set; }

    public override string TargetPathSegment => "/OnDelete";

    public string Action { get; } = Action;

    protected override EdmElement? Parent => ParentNavigationProperty;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Action", Action);
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var onDelete = new OnDelete(startElement.Get("Action"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is NavigationProperty navigationProperty)
        {
            navigationProperty.SetOnDelete(onDelete);
        }

        stack.Push(onDelete);
    }
}
