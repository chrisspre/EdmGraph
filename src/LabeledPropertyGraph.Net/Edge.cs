namespace LabeledPropertyGraph.Net;

/// <summary>
/// Represents an edge in a labeled property graph
/// </summary>
public record Edge(string Label, Node Source, Node Target)
{
    /// <summary>
    /// Gets the properties associated with this edge
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = [];


    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Edge AddProperty(string key, object value)
    {
        Properties[key] = value;
        return this;
    }

    /// <summary>
    /// Tries to get a property value of the specified type from the edge by its key.
    /// If the property exists and is of the correct type, it returns true and sets the out parameter to the value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetProperty<T>(string key, out T? value)
    {
        if (Properties.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }
        value = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"Edge({Label}): {Source.Label} -> {Target.Label}";
}