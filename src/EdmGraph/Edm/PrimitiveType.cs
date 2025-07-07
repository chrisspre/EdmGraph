namespace EdmGraph.Edm;

public sealed class PrimitiveType(string Name) : IEdmPropertyType, IEdmElement
{
    public string Name { get; internal set; } = Name;

    public string TargetPathSegment => $".{Name}";

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

    public IReadOnlyCollection<IEdmElement> Children => [];

    public IEdmElement? Parent => ParentEnumType;

    public EnumType? ParentEnumType { get; internal set; }

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
    }

    public LineInfo LineInfo { get; set; }

    public override string ToString()
    {
        var properties = GetBasicProperties();
        var propertyString = string.Join(", ", properties.Select(p => $"{p.Item1}={p.Item2}"));
        return $@"<{GetType().Name} {propertyString} \>";
    }
}
