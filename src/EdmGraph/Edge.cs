namespace EdmGraph;

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
    /// Adds a property to the edge with the specified key and value
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    /// <returns>This edge instance for method chaining</returns>
    public Edge AddProperty(string key, object value)
    {
        Properties[key] = value;
        return this;
    }

    /// <summary>
    /// Tries to get a property value of the specified type from the edge by its key
    /// </summary>
    /// <typeparam name="T">The expected type of the property value</typeparam>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value if found and of the correct type</param>
    /// <returns>True if the property exists and is of the specified type, false otherwise</returns>
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

    /// <summary>
    /// Returns a string representation of the edge showing its label and connected nodes
    /// </summary>
    /// <returns>A formatted string representation of the edge</returns>
    public override string ToString() => $"Edge({Label}): {Source.Label} -> {Target.Label}";
}