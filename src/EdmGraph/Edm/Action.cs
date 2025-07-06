namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Action
/// </summary>
public sealed class Action(
    string Name,
    bool IsBound = false,
    string? EntitySetPath = null) : EdmElement(Name)
{
    private readonly List<Parameter> _parameters = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Parameter> Parameters => _parameters;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public ReturnType? ReturnType { get; private set; }
    public Schema? ParentSchema { get; internal set; }

    public override string TargetPathSegment => $".{Name}";
    protected override EdmElement? Parent => ParentSchema;

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

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _parameters,
        .. _annotations,
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("IsBound", IsBound);
        if (EntitySetPath != null)
        {
            yield return ("EntitySetPath", EntitySetPath);
        }
    }
}
