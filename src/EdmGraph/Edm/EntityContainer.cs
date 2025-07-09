namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Container
/// </summary>
public sealed class EntityContainer(
    string Name,
    string? Extends = null) : EdmElement(Name), IEdmElementFactory<EntityContainer>
{
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

    public Schema ParentSchema { get; internal set; } = default!; // Set in ModelBuilder/Create

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentSchema;

    public override string TargetPathSegment => $".{Name}";

    public override IReadOnlyCollection<IEdmElement> Children => [
        .. _entitySets,
        .. _singletons,
        .. _actionImports,
        .. _functionImports,
        .. _annotations
    ];

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Name), Name);
        if (Extends != null)
        {
            yield return (nameof(Extends), Extends);
        }
    }

    #endregion

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

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public static EntityContainer Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var entityContainer = new EntityContainer(
            startElement.Get("Name"),
            startElement.Get("Extends", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (parent is Schema schema)
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

        return entityContainer;
    }
}
