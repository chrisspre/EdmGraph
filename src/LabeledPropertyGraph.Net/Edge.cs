namespace LabeledPropertyGraph.Net;

/// <summary>
/// Represents an edge in a labeled property graph
/// </summary>
public record Edge(string Label, Node Source, Node Target)
{
    public Dictionary<string, object> Properties { get; init; } = [];

    public Edge AddProperty(string key, object value)
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

    public override string ToString() => $"Edge({Label}): {Source.Label} -> {Target.Label}";
}