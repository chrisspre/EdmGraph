namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Navigation Property
/// </summary>
public sealed class NavigationProperty(
    string Name,
    bool? Nullable = default,
    bool ContainsTarget = false) : EdmElement(Name), IEntityTypeChildElement, IComplexTypeChildElement
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
}

/// <summary>
/// Represents a Referential Constraint on a Navigation Property
/// </summary>
public sealed class ReferentialConstraint() : EdmElement("ReferentialConstraint")
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
}

/// <summary>
/// Represents an On-Delete Action for a Navigation Property
/// </summary>
public sealed class OnDelete(
    string Action) : EdmElement("OnDelete")
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
}
