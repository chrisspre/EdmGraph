namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Return Type for an Action or Function
/// </summary>
public sealed class ReturnType(
    string Type,
    bool Nullable = true,
    int? MaxLength = null,
    bool Unicode = true,
    int? Precision = null,
    string? Scale = null,
    int? SRID = null) : EdmElement("ReturnType"), IEdmElementCreator
{
    public string Type { get; } = Type;
    public bool Nullable { get; } = Nullable;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public string? Scale { get; } = Scale;
    public int? SRID { get; } = SRID;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public EdmElement? ParentElement { get; internal set; }

    public override string TargetPathSegment => "/ReturnType";
    protected override EdmElement? Parent => ParentElement;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);


    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations,
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
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
        var returnType = new ReturnType(
            startElement.Get("Type"),
            startElement.Parse<bool>("Nullable", true))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Function function)
        {
            function.SetReturnType(returnType);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(returnType, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<IReturnType>(path);
        // });

        stack.Push(returnType);
    }
}
