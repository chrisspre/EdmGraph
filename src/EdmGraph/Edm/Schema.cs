namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Schema
/// </summary>
public sealed class Schema(
    string Namespace,
    string? Alias = null) : EdmElement(Namespace)
{
    public string? Alias { get; } = Alias;

    public string Namespace { get; } = Namespace;    

    private readonly List<EntityType> _entityTypes = [];
    private readonly List<ComplexType> _complexTypes = [];
    private readonly List<EnumType> _enumTypes = [];
    private readonly List<TypeDefinition> _typeDefinitions = [];
    private readonly List<Term> _terms = [];
    private readonly List<Action> _actions = [];
    private readonly List<Function> _functions = [];
    private readonly List<EntityContainer> _entityContainers = [];
    private readonly List<Annotations> _annotations = [];

    public IReadOnlyCollection<EntityType> EntityTypes => _entityTypes;
    public IReadOnlyCollection<ComplexType> ComplexTypes => _complexTypes;
    public IReadOnlyCollection<EnumType> EnumTypes => _enumTypes;
    public IReadOnlyCollection<TypeDefinition> TypeDefinitions => _typeDefinitions;
    public IReadOnlyCollection<Term> Terms => _terms;
    public IReadOnlyCollection<Action> Actions => _actions;
    public IReadOnlyCollection<Function> Functions => _functions;
    public IReadOnlyCollection<EntityContainer> EntityContainers => _entityContainers;
    public IReadOnlyCollection<Annotations> Annotations => _annotations;

    public DataServices? ParentDataServices { get; internal set; }

    public override string TargetPathSegment => $"{Namespace}";
    protected override EdmElement? Parent => ParentDataServices;

    internal void AddEntityType(EntityType entityType) 
    {
        entityType.ParentSchema = this;
        _entityTypes.Add(entityType);
    }
    
    internal void AddComplexType(ComplexType complexType) 
    {
        complexType.ParentSchema = this;
        _complexTypes.Add(complexType);
    }
    internal void AddEnumType(EnumType enumType) 
    {
        enumType.ParentSchema = this;
        _enumTypes.Add(enumType);
    }
    
    internal void AddTypeDefinition(TypeDefinition typeDefinition) 
    {
        typeDefinition.ParentSchema = this;
        _typeDefinitions.Add(typeDefinition);
    }
    
    internal void AddTerm(Term term) 
    {
        term.ParentSchema = this;
        _terms.Add(term);
    }
    
    internal void AddAction(Action action) 
    {
        action.ParentSchema = this;
        _actions.Add(action);
    }
    
    internal void AddFunction(Function function) 
    {
        function.ParentSchema = this;
        _functions.Add(function);
    }
    
    internal void AddEntityContainer(EntityContainer entityContainer) 
    {
        entityContainer.ParentSchema = this;
        _entityContainers.Add(entityContainer);
    }
    
    internal void AddAnnotations(Annotations annotations) 
    {
        annotations.ParentSchema = this;
        _annotations.Add(annotations);
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _entityTypes,
        .. _complexTypes,
        .. _enumTypes,
        .. _typeDefinitions,
        .. _terms,
        .. _actions,
        .. _functions,
        .. _entityContainers,
        .. _annotations
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        if (Alias != null)
        {
            yield return ("Alias", Alias);
        }
    }

}
