namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Set
/// </summary>
public sealed class EntitySet(
    string Name,
    bool IncludeInServiceDocument = true) : EdmElement(Name), IEdmElementFactory<EntitySet>
{
    public bool IncludeInServiceDocument { get; } = IncludeInServiceDocument;


    public EntityType EntityType { get; private set; } = default!; // Set by ModelBuilder

    public EntityContainer ParentEntityContainer { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<NavigationPropertyBinding> _navigationPropertyBindings = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<NavigationPropertyBinding> NavigationPropertyBindings => _navigationPropertyBindings;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

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

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEntityContainer;

    public override IReadOnlyCollection<IEdmElement> Children => [
        .. _navigationPropertyBindings,
        .. _annotations
    ];

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("EntityType", EntityType);
        yield return ("IncludeInServiceDocument", IncludeInServiceDocument);
    }

    #endregion

    public static EntitySet Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var entitySet = new EntitySet(
           startElement.Get("Name"),
           
           startElement.Parse<bool>("IncludeInServiceDocument", true));

        if (parent is EntityContainer entityContainer)
        {
            entityContainer.AddEntitySet(entitySet);
        }

        // Add deferred action for EntityType property resolution
        var entityTypePath = startElement.Get("EntityType");
        actions.Add(Priorities.Type, entitySet, entityTypePath, static (element, path, resolver) => { 
            element.EntityType = resolver.Resolve<EntityType>(path);
        });

        return entitySet;
    }
}
