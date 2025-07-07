namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Singleton
/// </summary>
public sealed class Singleton(
    string Name,
    string Type,
    bool Nullable = false) : IEdmElement, IEdmElementCreator<Singleton>
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public bool Nullable { get; } = Nullable;

    public EntityContainer? ParentEntityContainer { get; internal set; }

    private readonly List<NavigationPropertyBinding> _navigationPropertyBindings = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<NavigationPropertyBinding> NavigationPropertyBindings => _navigationPropertyBindings;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    // IEdmElement implementation
    public string TargetPathSegment => $"/{Name}";
    public IEdmElement? Parent => ParentEntityContainer;
    public IReadOnlyCollection<IEdmElement> Children => _annotations.Cast<IEdmElement>().ToList();
    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

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

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Type", Type);
        yield return ("Nullable", Nullable);
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static Singleton Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var singleton = new Singleton(
            startElement.Get("Name"),
            startElement.Get("Type"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityContainer entityContainer:
                    entityContainer.AddSingleton(singleton);
                    break;
            }
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(singleton, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<EntityType>(path);
        // });

        return singleton;
    }
}
