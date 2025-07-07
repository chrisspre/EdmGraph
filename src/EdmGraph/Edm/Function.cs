namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Function
/// </summary>
public sealed class Function(
    string Name,
    bool IsBound = false,
    string? EntitySetPath = null,
    bool IsComposable = false) : IEdmElement, IEdmElementCreator<Function>
{
    public string Name { get; } = Name;
    public bool IsBound { get; } = IsBound;
    public string? EntitySetPath { get; } = EntitySetPath;
    public bool IsComposable { get; } = IsComposable;

    private readonly List<Parameter> _parameters = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Parameter> Parameters => _parameters;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public ReturnType? ReturnType { get; private set; }
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
            children.AddRange(_parameters.Cast<IEdmElement>());
            children.AddRange(_annotations.Cast<IEdmElement>());
            if (ReturnType != null)
            {
                // TODO: Add ReturnType back after it's refactored
                // children.Add(ReturnType);
            }
            return children;
        }
    }

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

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("IsBound", IsBound);
        if (EntitySetPath != null)
        {
            yield return ("EntitySetPath", EntitySetPath);
        }
        yield return ("IsComposable", IsComposable);
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static Function Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var function = new Function(
            startElement.Get("Name"),
            startElement.Parse<bool>("IsBound"),
            startElement.Get("EntitySetPath", null),
            startElement.Parse<bool>("IsComposable"));

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Schema schema:
                    schema.AddFunction(function);
                    break;
            }
        }

        stack.Push(function);
        return function;
    }
}
