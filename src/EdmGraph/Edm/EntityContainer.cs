namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Entity Container
/// </summary>
public sealed class EntityContainer(
    string Name,
    string? Extends = null) : EdmElement(Name)
{
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

    public override string TargetPathSegment => $".{Name}";
    protected override EdmElement? Parent => ParentSchema;

    public string?
     Extends
    { get; } = Extends;

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


    public override IReadOnlyCollection<EdmElement> Children => [
        .. _entitySets,
        .. _singletons,
        .. _actionImports,
        .. _functionImports,
        .. _annotations,
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        if (Extends != null)
        {
            yield return ("Extends", Extends);
        }
    }
}
