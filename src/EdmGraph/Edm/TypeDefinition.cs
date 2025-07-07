using Serilog;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Type Definition
/// </summary>
public sealed class TypeDefinition(
    string Name,
    string UnderlyingType,
    int? MaxLength = null,
    bool Unicode = true,
    int? Precision = null,
    string? Scale = null,
    int? SRID = null) : IEdmElement, IEdmElementCreator<TypeDefinition>
{
    public string Name { get; } = Name;
    public string UnderlyingType { get; } = UnderlyingType;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public string? Scale { get; } = Scale;
    public int? SRID { get; } = SRID;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public Schema? ParentSchema { get; internal set; }
    
    // IEdmElement implementation
    public string TargetPathSegment => $".{Name}";
    public IEdmElement? Parent => ParentSchema;
    public IReadOnlyCollection<IEdmElement> Children => _annotations.Cast<IEdmElement>().ToList();
    public LineInfo LineInfo { get; set; }
    
    public string TargetPath => Parent switch
    {
        null => Name,
        _ => Parent.TargetPath + TargetPathSegment
    };

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("UnderlyingType", UnderlyingType);
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

    public override string ToString()
    {
        var properties = GetBasicProperties()
            .Select(p => $"{p.Item1}: {p.Item2}")
            .ToArray();

        return $"{GetType().Name} {{ {string.Join(", ", properties)} }}";
    }

    public static TypeDefinition Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var typeDefinition = new TypeDefinition(
            startElement.Get("Name"),
            startElement.Get("UnderlyingType"),
            startElement.ParseNullable<int>("MaxLength"),
            startElement.Parse<bool>("Unicode", true),
            startElement.ParseNullable<int>("Precision"),
            startElement.Get("Scale", null),
            startElement.ParseNullable<int>("SRID")
        )
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Schema schema:
                    schema.AddTypeDefinition(typeDefinition);
                    break;
                default:
                    Log.Error("TypeDefinition {Name} can not be added to {parent}.", typeDefinition.Name, parent);
                    break;
            }
        }

        // TODO: Add deferred action for UnderlyingType property resolution
        // actions.Add(typeDefinition, startElement.Get("UnderlyingType"), static (element, path, resolver) => { 
        //     element.UnderlyingType = resolver.Resolve<IUnderlyingType>(path);
        // });

        stack.Push(typeDefinition);
        return typeDefinition;
    }
}
