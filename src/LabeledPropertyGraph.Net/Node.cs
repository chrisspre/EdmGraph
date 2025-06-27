namespace LabeledPropertyGraph.Net;

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



    /// <summary>
    /// Single-step traversal: follows all outgoing edges from this node
    /// </summary>
    public IEnumerable<Node> TraverseOut()
    {
        return OutgoingEdges.Select(e => e.Target);
    }

    /// <summary>
    /// Single-step traversal: follows outgoing edges with specific label from this node
    /// </summary>
    public IEnumerable<Node> TraverseOut(string edgeLabel)
    {
        return OutgoingEdges.Where(e => e.Label == edgeLabel).Select(e => e.Target);
    }

    /// <summary>
    /// Single-step traversal: follows all incoming edges to this node
    /// </summary>
    public IEnumerable<Node> TraverseIn()
    {
        return IncomingEdges.Select(e => e.Source);
    }

    /// <summary>
    /// Single-step traversal: follows incoming edges with specific label to this node
    /// </summary>
    public IEnumerable<Node> TraverseIn(string edgeLabel)
    {
        return IncomingEdges.Where(e => e.Label == edgeLabel).Select(e => e.Source);
    }

    public override string ToString()
    {
        var properties = Properties.Count == 0 ? "" :
            $" {{{string.Join(", ", Properties.Select(p => $"{p.Key}: {p.Value}"))}}}";
        return $"<{Label}{properties}>";
    }
}