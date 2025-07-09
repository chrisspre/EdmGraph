namespace EdmGraph.Edm;

public abstract class EdmElement(string Name) : IEdmElement
{
    public string Name { get; } = Name;

    public LineInfo LineInfo { get; set; } = new();

    public string? TargetPath => Parent?.TargetPath + TargetPathSegment;

    public override string ToString() =>
        $"<{GetType().Name} {string.Join(", ", GetBasicProperties().Select(p => $"{p.Item1}={p.Item2}"))} \\>";

    // abstract members 

    public abstract IEdmElement Parent { get; }

    public abstract IReadOnlyCollection<IEdmElement> Children { get; }

    public abstract string TargetPathSegment { get; }

    public abstract IEnumerable<(string Name, object? Value)> GetBasicProperties();

    protected static IEnumerable<(string, object?)> NonNull(string name, object? value)
    {
        if (value != null)
        {
            yield return (name, value);
        }
    }
}
