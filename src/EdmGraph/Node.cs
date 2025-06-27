namespace EdmGraph;

/// <summary>
/// Represents a node in a labeled property graph with directional adjacency lists
/// </summary>
public record Node(string Label)
{
    /// <summary>
    /// Gets the properties associated with this node
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = [];

    /// <summary>
    /// Gets the list of outgoing edges from this node
    /// </summary>
    public List<Edge> OutgoingEdges { get; init; } = [];

    /// <summary>
    /// Gets the list of incoming edges to this node
    /// </summary>
    public List<Edge> IncomingEdges { get; init; } = [];

    /// <summary>
    /// Adds a property to this node
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    /// <returns>This node instance for method chaining</returns>
    public Node AddProperty(string key, object value)
    {
        Properties[key] = value;
        return this;
    }

    /// <summary>
    /// Tries to get a property value with the specified type
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

    /// <summary>
    /// Returns a string representation of the node including its label and properties
    /// </summary>
    /// <returns>A formatted string representation of the node</returns>
    public override string ToString()
    {
        var properties = Properties.Count == 0 ? "" :
            $" {{{string.Join(", ", Properties.Select(p => $"{p.Key}: {p.Value}"))}}}";
        return $"<{Label}{properties}>";
    }
}