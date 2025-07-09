namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Singleton
/// </summary>
public sealed class Singleton(
    string Name,
    string Type,
    bool Nullable = false) : EdmElement(Name), IEdmElementFactory<Singleton>
{
    public string Type { get; } = Type;
    public bool Nullable { get; } = Nullable;

    public EntityContainer ParentEntityContainer { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<NavigationPropertyBinding> _navigationPropertyBindings = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<NavigationPropertyBinding> NavigationPropertyBindings => _navigationPropertyBindings;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEntityContainer;

    public override IReadOnlyCollection<IEdmElement> Children => [
        .. _navigationPropertyBindings,
        .. _annotations
    ];

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Name), Name);
        yield return (nameof(Type), Type);
        yield return (nameof(Nullable), Nullable);
    }

    #endregion

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

    public static Singleton Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var singleton = new Singleton(
            startElement.Get("Name"),
            startElement.Get("Type"))
        {
            LineInfo = startElement.LineInfo
        };

        if (parent is EntityContainer entityContainer)
        {
            entityContainer.AddSingleton(singleton);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(singleton, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<EntityType>(path);
        // });

        return singleton;
    }
}
