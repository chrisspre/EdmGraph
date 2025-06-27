namespace EdmGraph;

/// <summary>
/// Represents a node in a labeled property graph with directional adjacency lists
/// </summary>
public record Node(string Label)
{
    public Dictionary<string, object> Properties { get; init; } = [];
    public List<Edge> OutgoingEdges { get; init; } = [];
    public List<Edge> IncomingEdges { get; init; } = [];

    public Node AddProperty(string key, object value)
    {
        Properties[key] = value;
        return this;
    }

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

    public override string ToString() => $"Node({Label})";
}