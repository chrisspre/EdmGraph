namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Set
/// </summary>
public sealed class EntitySet(
    string Name,
    bool IncludeInServiceDocument = true) : IEdmElement, IEdmElementFactory<EntitySet>
{
    public bool IncludeInServiceDocument { get; } = IncludeInServiceDocument;


    public EntityType EntityType { get; private set; } = default!; // Set by ModelBuilder

    public EntityContainer? ParentEntityContainer { get; internal set; }

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

    #region IEdmElement

    string? IEdmElement.Name => Name;
    IEdmElement? IEdmElement.Parent => ParentEntityContainer;
    string IEdmElement.TargetPathSegment => $"/{Name}";
    LineInfo IEdmElement.LineInfo { get; set; } = new();
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [
        .. _navigationPropertyBindings,
        .. _annotations
    ];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Name", ((IEdmElement)this).TargetPath);
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
