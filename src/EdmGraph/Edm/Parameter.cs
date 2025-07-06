using Serilog;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Parameter for an Action or Function
/// </summary>
public sealed class Parameter(
    string Name,
    bool Nullable = true,
    int? MaxLength = null,
    bool Unicode = true,
    int? Precision = null,
    string? Scale = null,
    int? SRID = null) : EdmElement(Name), IEdmElementCreator
{
    public IEdmPropertyType Type { get; internal set; } = default!; // Set by ModelBuilder
    public bool Nullable { get; } = Nullable;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public string? Scale { get; } = Scale;
    public int? SRID { get; } = SRID;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public EdmElement? ParentElement { get; internal set; }

    public override string TargetPathSegment => $"/{Name}";
    protected override EdmElement? Parent => ParentElement;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);
    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Type", Type);
        yield return ("Nullable", Nullable);
        if (MaxLength.HasValue)
        {
            yield return ("MaxLength", MaxLength.Value);
        }
        yield return ("Unicode", Unicode);
        if (Precision.HasValue)
        {
            yield return ("Precision", Precision.Value);
        }
        if (Scale != null)
        {
            yield return ("Scale", Scale);
        }
        if (SRID.HasValue)
        {
            yield return ("SRID", SRID.Value);
        }
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var parameter = new Parameter(
            startElement.Get("Name"),
            startElement.Parse<bool>("Nullable", true))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Function function:
                    function.AddParameter(parameter);
                    break;
                case Action action:
                    action.AddParameter(parameter);
                    break;
            }
        }

        actions.Add(Priorities.Type, parameter, startElement.Get("Type"), SetType);
        stack.Push(parameter);

        // static helper method for the deferred action to set the Type property of Parameter
        static void SetType(Parameter parameter, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                // element.IsCollection = true;
            }
            parameter.Type = resolver.Resolve<IEdmPropertyType>(path);
            if (parameter.Type is null)
            {
                Log.Warning("Element '{path}' not found while trying setting the Type property of {element}", path, parameter);
            }
        }
    }
}
