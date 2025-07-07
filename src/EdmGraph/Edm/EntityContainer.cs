namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Container
/// </summary>
public sealed class EntityContainer(
    string Name,
    string? Extends = null) : IEdmElement, IEdmElementCreator<EntityContainer>
{
    public string Name { get; } = Name;
    public string? Extends { get; } = Extends;

    private readonly List<EntitySet> _entitySets = [];
    private readonly List<Singleton> _singletons = [];
    private readonly List<ActionImport> _actionImports = [];
    private readonly List<FunctionImport> _functionImports = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<EntitySet> EntitySets => _entitySets;
    public IReadOnlyCollection<Singleton> Singletons => _singletons;
    public IReadOnlyCollection<ActionImport> ActionImports => _actionImports;
    public IReadOnlyCollection<FunctionImport> FunctionImports => _functionImports;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public Schema? ParentSchema { get; internal set; }

    // IEdmElement implementation
    public string TargetPathSegment => $".{Name}";
    public IEdmElement? Parent => ParentSchema;
    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

    public IReadOnlyCollection<IEdmElement> Children 
    {
        get
        {
            var children = new List<IEdmElement>();
            // TODO: Add children back after other classes are refactored
            // children.AddRange(_entitySets.Cast<IEdmElement>());
            // children.AddRange(_singletons.Cast<IEdmElement>());
            // children.AddRange(_actionImports.Cast<IEdmElement>());
            // children.AddRange(_functionImports.Cast<IEdmElement>());
            children.AddRange(_annotations.Cast<IEdmElement>());
            return children;
        }
    }

    internal void AddEntitySet(EntitySet entitySet)
    {
        entitySet.ParentEntityContainer = this;
        _entitySets.Add(entitySet);
    }

    internal void AddSingleton(Singleton singleton)
    {
        singleton.ParentEntityContainer = this;
        _singletons.Add(singleton);
    }

    internal void AddActionImport(ActionImport actionImport)
    {
        actionImport.ParentEntityContainer = this;
        _actionImports.Add(actionImport);
    }

    internal void AddFunctionImport(FunctionImport functionImport)
    {
        functionImport.ParentEntityContainer = this;
        _functionImports.Add(functionImport);
    }

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        if (Extends != null)
        {
            yield return ("Extends", Extends);
        }
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static EntityContainer Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var entityContainer = new EntityContainer(
            startElement.Get("Name"),
            startElement.Get("Extends", null));

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddEntityContainer(entityContainer);
        }

        // TODO: Add deferred action for Extends property resolution if Extends is not null
        var extends = startElement.Get("Extends", null);
        if (extends is not null)
        {
            // actions.Add(entityContainer, extends, static (element, path, resolver) => { 
            //     element.Extends = resolver.Resolve<EntityContainer>(path);
            // });
        }

        stack.Push(entityContainer);
        return entityContainer;
    }
}
