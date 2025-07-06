namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Set
/// </summary>
public sealed class EntitySet(
    string Name,
    string EntityType,
    bool IncludeInServiceDocument = true) : EdmElement(Name)
{
    public string EntityType { get; } = EntityType;
    public bool IncludeInServiceDocument { get; } = IncludeInServiceDocument;
    
    public EntityContainer? ParentEntityContainer { get; internal set; }
    
    private readonly List<NavigationPropertyBinding> _navigationPropertyBindings = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<NavigationPropertyBinding> NavigationPropertyBindings => _navigationPropertyBindings;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => $"/{Name}";
    protected override EdmElement? Parent => ParentEntityContainer;

    internal void AddNavigationPropertyBinding(NavigationPropertyBinding binding)
    {
        _navigationPropertyBindings.Add(binding);
        binding.ParentElement = this;
    }
    
    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _navigationPropertyBindings,
        .. _annotations
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("EntityType", EntityType);
        yield return ("IncludeInServiceDocument", IncludeInServiceDocument);
    }
}
