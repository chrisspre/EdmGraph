namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Function
/// </summary>
public sealed class Function(
    string Name,
    bool IsBound = false,
    string? EntitySetPath = null,
    bool IsComposable = false) : EdmElement(Name)
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


    public override IReadOnlyCollection<EdmElement> Children => [
        .. _parameters,
        .. _annotations,
    ];

    public bool IsBound { get; } = IsBound;
    public string? EntitySetPath { get; } = EntitySetPath;
    public bool IsComposable { get; } = IsComposable;

    internal void SetReturnType(ReturnType returnType) 
    {
        returnType.ParentElement = this;
        ReturnType = returnType;
    }

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("IsBound", IsBound);
        if (EntitySetPath != null)
        {
            yield return ("EntitySetPath", EntitySetPath);
        }
        yield return ("IsComposable", IsComposable);
    }
}
