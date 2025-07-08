namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Action
/// </summary>
public sealed class Action(
    string Name,
    bool IsBound = false,
    string? EntitySetPath = null) : EdmElement(Name), IEdmElementFactory<Action>
{
    public bool IsBound { get; } = IsBound;
    public string? EntitySetPath { get; } = EntitySetPath;

    private readonly List<Parameter> _parameters = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Parameter> Parameters => _parameters;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public ReturnType? ReturnType { get; private set; }
    public Schema ParentSchema { get; internal set; } = default!; // Set in ModelBuilder/Create
    
    internal void AddParameter(Parameter parameter) 
    {
        parameter.ParentElement = this;
        _parameters.Add(parameter);
    }
    
    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);
    
    internal void SetReturnType(ReturnType returnType) 
    {
        returnType.ParentElement = this;
        ReturnType = returnType;
    }

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentSchema;

    public override IReadOnlyCollection<IEdmElement> Children 
    {
        get
        {
            var children = new List<IEdmElement>();
            // TODO: Add parameters back after Parameter is refactored
            // children.AddRange(_parameters.Cast<IEdmElement>());
            children.AddRange(_annotations.Cast<IEdmElement>());
            if (ReturnType != null)
            {
                // TODO: Add ReturnType back after it's refactored
                // children.Add(ReturnType);
            }
            return children;
        }
    }

    public override string TargetPathSegment => $".{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("IsBound", IsBound);
        if (EntitySetPath != null)
        {
            yield return ("EntitySetPath", EntitySetPath);
        }
    }

    #endregion

    public static Action Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var action = new Action(
            startElement.Get("Name"),
            startElement.Parse<bool>("IsBound"),
            startElement.Get("EntitySetPath", null));

        if (parent is Schema schema)
        {
            schema.AddAction(action);
        }

        return action;
    }
}
