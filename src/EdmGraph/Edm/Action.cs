namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Action
/// </summary>
public sealed class Action(
    string Name,
    bool IsBound = false,
    string? EntitySetPath = null) : IEdmElement, IEdmElementCreator<Action>
{
    public string Name { get; } = Name;
    public bool IsBound { get; } = IsBound;
    public string? EntitySetPath { get; } = EntitySetPath;

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
    }

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static Action Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var action = new Action(
            startElement.Get("Name"),
            startElement.Parse<bool>("IsBound"),
            startElement.Get("EntitySetPath", null));

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Schema schema:
                    schema.AddAction(action);
                    break;
            }
        }

        stack.Push(action);
        return action;
    }
}
