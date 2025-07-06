

namespace EdmGraph.Edm;

/// <summary>
/// Represents the root EDMX element
/// </summary>
public sealed class Edmx(string Version = "4.0") : EdmElement("Edmx")
{
    private readonly List<Reference> _references = [];

    public IReadOnlyCollection<Reference> References => _references;
    public DataServices? DataServices { get; private set; }

    public override string TargetPathSegment => "";

    public string Version { get; } = Version;

    protected override EdmElement? Parent => null;

    internal void AddReference(Reference reference)
    {
        _references.Add(reference);
        reference.ParentEdmx = this;
    }

    internal void SetDataServices(DataServices dataServices)
    {
        DataServices = dataServices;
        dataServices.ParentEdmx = this;
    }


    public override IReadOnlyCollection<EdmElement> Children => [
        .. _references,
        .. (DataServices is null ? Enumerable.Empty<EdmElement>() : [DataServices])
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Version", Version);
    }
}

/// <summary>
/// Represents the EDMX DataServices element
/// </summary>
public sealed class DataServices() : EdmElement("DataServices")
{
    public Edmx? ParentEdmx { get; internal set; }

    private readonly List<Schema> _schemas = [];

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Schema> Schemas => _schemas;

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => "";


    internal void AddSchema(Schema schema)
    {
        schema.ParentDataServices = this;
        _schemas.Add(schema);
    }

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    protected override EdmElement? Parent => ParentEdmx;

    public override IReadOnlyCollection<EdmElement> Children => [.. _schemas];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        // DataServices doesn't have additional basic properties beyond its name
        yield break;
    }
}
