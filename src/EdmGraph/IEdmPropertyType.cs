namespace EdmGraph;

/// <summary>
/// a non-empty sequence of property names that used to reference a chain of properties across structured types.
/// </summary>
public class PropertyPath(IProperty initialProperty) : IEnumerable<IProperty>
{
    private readonly List<IProperty> _segments = [initialProperty];

    public void AddSegment(IProperty segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        _segments.Add(segment);
    }

    public IEnumerator<IProperty> GetEnumerator() => _segments.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => string.Join("/", _segments.Select(p => p.Name));
}


/// <summary>
/// one of the types that can be used as a property type in the EDM model.
/// This includes primitive types, complex types, entity types, and collection types.
/// </summary>
public interface IEdmPropertyType : IEdmElement { }

public interface IProperty : IEdmElement
{
    /// <summary>
    /// Gets the type of the property
    /// </summary>
    IEdmPropertyType Type { get; }

    /// <summary>
    /// Indicates whether this property is a collection
    /// </summary>
    bool IsCollection { get; }
}
