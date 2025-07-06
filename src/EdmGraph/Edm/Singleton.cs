namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Singleton
/// </summary>
public sealed class Singleton(
    string Name,
    string Type,
    bool Nullable = false) : EdmElement(Name), IEdmElementCreator
{
    public string Type { get; } = Type;
    public bool Nullable { get; } = Nullable;

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

    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Type", Type);
        yield return ("Nullable", Nullable);
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var singleton = new Singleton(
            startElement.Get("Name"),
            startElement.Get("Type"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityContainer entityContainer)
        {
            entityContainer.AddSingleton(singleton);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(singleton, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<EntityType>(path);
        // });

        stack.Push(singleton);
    }
}
