

namespace EdmGraph.Edm;

/// <summary>
/// Represents the root EDMX element
/// </summary>
public sealed class Edmx(string Version = "4.0") : IEdmElement
{
    private readonly List<Reference> _references = [];

    public IReadOnlyCollection<Reference> References => _references;
    public DataServices? DataServices { get; private set; }

    public string Name { get; internal set; } = "Edmx";

    public string TargetPathSegment => null!; // The Edmx element does not have a target path segment

    public string? TargetPath => TargetPathSegment is null ? null : Parent?.TargetPath + TargetPathSegment;

    public string Version { get; } = Version;

    public IEdmElement? Parent => null;

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

    public IReadOnlyCollection<IEdmElement> Children => [
        .. _references,
        .. (DataServices is null ? Enumerable.Empty<IEdmElement>() : [DataServices])
    ];

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Version", Version);
    }

    public LineInfo LineInfo { get; set; }

    public override string ToString()
    {
        var properties = GetBasicProperties();
        var propertyString = string.Join(", ", properties.Select(p => $"{p.Item1}={p.Item2}"));
        return $@"<{GetType().Name} {propertyString} \>";
    }
}

/// <summary>
/// Represents the EDMX DataServices element
/// </summary>
public sealed class DataServices() : IEdmElement
{
    public Edmx? ParentEdmx { get; internal set; }

    private readonly List<Schema> _schemas = [];

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Schema> Schemas => _schemas;

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public string Name { get; internal set; } = "DataServices";

    public string TargetPathSegment => "";

    public string? TargetPath
    {
        get
        {
            if (TargetPathSegment is null)
            {
                return null;
            }
            return Parent?.TargetPath + TargetPathSegment;
        }
    }

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

    public IEdmElement? Parent => ParentEdmx;

    public IReadOnlyCollection<IEdmElement> Children => [.. _schemas];

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        // DataServices doesn't have additional basic properties beyond its name
        yield break;
    }

    public LineInfo LineInfo { get; set; }

    public override string ToString()
    {
        var properties = GetBasicProperties();
        var propertyString = string.Join(", ", properties.Select(p => $"{p.Item1}={p.Item2}"));
        return $@"<{GetType().Name} {propertyString} \>";
    }
}
