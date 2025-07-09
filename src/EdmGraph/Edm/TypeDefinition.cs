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
    int? SRID = null) : EdmElement(Name), IEdmElementFactory<TypeDefinition>
{
    public string UnderlyingType { get; } = UnderlyingType;
    public int? MaxLength { get; } = MaxLength;
    public bool Unicode { get; } = Unicode;
    public int? Precision { get; } = Precision;
    public string? Scale { get; } = Scale;
    public int? SRID { get; } = SRID;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public Schema ParentSchema { get; internal set; } = default!; // Set in ModelBuilder/Create
    
    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentSchema;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => $".{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Name), Name);
        yield return (nameof(UnderlyingType), UnderlyingType);
        if (MaxLength.HasValue)
        {
            yield return (nameof(MaxLength), MaxLength.Value);
        }
        yield return (nameof(Unicode), Unicode);
        if (Precision.HasValue)
        {
            yield return (nameof(Precision), Precision.Value);
        }
        if (Scale != null)
        {
            yield return (nameof(Scale), Scale);
        }
        if (SRID.HasValue)
        {
            yield return (nameof(SRID), SRID.Value);
        }
    }

    #endregion

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public static TypeDefinition Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
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

        if (parent is Schema schema)
        {
            schema.AddTypeDefinition(typeDefinition);
        }
        else
        {
            Log.Error("TypeDefinition {Name} can not be added to {parent}.", typeDefinition.Name, parent);
        }

        // TODO: Add deferred action for UnderlyingType property resolution
        // actions.Add(typeDefinition, startElement.Get("UnderlyingType"), static (element, path, resolver) => { 
        //     element.UnderlyingType = resolver.Resolve<IUnderlyingType>(path);
        // });
                
        return typeDefinition;
    }
}
