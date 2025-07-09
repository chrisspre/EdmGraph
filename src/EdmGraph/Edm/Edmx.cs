
namespace EdmGraph.Edm;

/// <summary>
/// Represents the root EDMX element
/// </summary>
public sealed class Edmx(string Version = "4.0") : EdmElement("Edmx")
{
    private readonly List<Reference> _references = [];

    public IReadOnlyCollection<Reference> References => _references;
    public DataServices? DataServices { get; private set; }

    public string Version { get; } = Version;

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => null!; // Root element has no parent

    public override string TargetPathSegment => ""; // Root element has empty path segment

    public override IReadOnlyCollection<IEdmElement> Children => DataServices is null 
        ? _references.Cast<IEdmElement>().ToArray()
        : [.. _references, DataServices];

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Version), Version);
    }

    #endregion

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
}

/// <summary>
/// Represents the EDMX DataServices element
/// </summary>
public sealed class DataServices() : EdmElement("DataServices")
{
    public Edmx ParentEdmx { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<Schema> _schemas = [];

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Schema> Schemas => _schemas;

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEdmx;

    public override string TargetPathSegment => "";

    public override IReadOnlyCollection<IEdmElement> Children => [.. _schemas];

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        // DataServices doesn't have additional basic properties beyond its name
        yield break;
    }

    #endregion

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
}
