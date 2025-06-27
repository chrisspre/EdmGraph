namespace EdmGraph;

/// <summary>
/// Represents a labeled property graph with nodes and edges, providing indexing and traversal capabilities
/// </summary>
public class Graph
{
    private readonly HashSet<Node> _nodes = [];
    private readonly HashSet<Edge> _edges = [];
    private readonly Dictionary<string, List<Node>> _nodesByLabel = [];
    private readonly Dictionary<string, List<Edge>> _edgesByLabel = [];

    public IEnumerable<Node> Nodes => _nodes;
    public IEnumerable<Edge> Edges => _edges;

    /// <summary>
    /// Creates and adds a new node to the graph using the Try-Pattern
    /// </summary>
    public bool TryCreateNode(string label, out Node? node)
    {
        node = new Node(label);

        if (!_nodes.Add(node))
        {
            node = null;
            return false;
        }

        if (!_nodesByLabel.TryGetValue(label, out var labelList))
        {
            labelList = [];
            _nodesByLabel[label] = labelList;
        }
        labelList.Add(node);

        return true;
    }

    /// <summary>
    /// Adds an existing node to the graph using the Try-Pattern
    /// </summary>
    public bool TryAddNode(Node node)
    {
        if (!_nodes.Add(node))
            return false;

        if (!_nodesByLabel.TryGetValue(node.Label, out var labelList))
        {
            labelList = [];
            _nodesByLabel[node.Label] = labelList;
        }
        labelList.Add(node);

        return true;
    }

    /// <summary>
    /// Creates and adds a new edge to the graph using the Try-Pattern
    /// </summary>
    public bool TryCreateEdge(string label, Node fromNode, Node toNode, out Edge? edge)
    {
        edge = null;

        if (!_nodes.Contains(fromNode) || !_nodes.Contains(toNode))
            return false;

        edge = new Edge(label, fromNode, toNode);

        if (!_edges.Add(edge))
        {
            edge = null;
            return false;
        }

        // Update adjacency lists
        fromNode.OutgoingEdges.Add(edge);
        toNode.IncomingEdges.Add(edge);

        // Update label index
        if (!_edgesByLabel.TryGetValue(label, out var labelList))
        {
            labelList = [];
            _edgesByLabel[label] = labelList;
        }
        labelList.Add(edge);

        return true;
    }

    /// <summary>
    /// Adds an existing edge to the graph using the Try-Pattern
    /// </summary>
    public bool TryAddEdge(Edge edge)
    {
        if (!_nodes.Contains(edge.From) || !_nodes.Contains(edge.To))
            return false;

        if (!_edges.Add(edge))
            return false;

        // Update adjacency lists
        edge.From.OutgoingEdges.Add(edge);
        edge.To.IncomingEdges.Add(edge);

        // Update label index
        if (!_edgesByLabel.TryGetValue(edge.Label, out var labelList))
        {
            labelList = [];
            _edgesByLabel[edge.Label] = labelList;
        }
        labelList.Add(edge);

        return true;
    }

    /// <summary>
    /// Checks if a node exists in the graph
    /// </summary>
    public bool ContainsNode(Node node) => _nodes.Contains(node);

    /// <summary>
    /// Checks if an edge exists in the graph
    /// </summary>
    public bool ContainsEdge(Edge edge) => _edges.Contains(edge);

    /// <summary>
    /// Gets all nodes with the specified label
    /// </summary>
    public IEnumerable<Node> GetNodesByLabel(string label) =>
        _nodesByLabel.TryGetValue(label, out var nodes) ? nodes : [];

    /// <summary>
    /// Gets all edges with the specified label
    /// </summary>
    public IEnumerable<Edge> GetEdgesByLabel(string label) =>
        _edgesByLabel.TryGetValue(label, out var edges) ? edges : [];

    /// <summary>
    /// Single-step traversal: follows outgoing edges from a node
    /// </summary>
    public IEnumerable<Node> TraverseOut(Node node, string? edgeLabel = null)
    {
        if (!_nodes.Contains(node))
            return [];

        var edges = edgeLabel == null
            ? node.OutgoingEdges
            : node.OutgoingEdges.Where(e => e.Label == edgeLabel);

        return edges.Select(e => e.To);
    }

    /// <summary>
    /// Single-step traversal: follows incoming edges to a node
    /// </summary>
    public IEnumerable<Node> TraverseIn(Node node, string? edgeLabel = null)
    {
        if (!_nodes.Contains(node))
            return [];

        var edges = edgeLabel == null
            ? node.IncomingEdges
            : node.IncomingEdges.Where(e => e.Label == edgeLabel);

        return edges.Select(e => e.From);
    }

    /// <summary>
    /// Gets all outgoing edges from a node
    /// </summary>
    public IEnumerable<Edge> GetOutgoingEdges(Node node, string? edgeLabel = null)
    {
        if (!_nodes.Contains(node))
            return [];

        return edgeLabel == null
            ? node.OutgoingEdges
            : node.OutgoingEdges.Where(e => e.Label == edgeLabel);
    }

    /// <summary>
    /// Gets all incoming edges to a node
    /// </summary>
    public IEnumerable<Edge> GetIncomingEdges(Node node, string? edgeLabel = null)
    {
        if (!_nodes.Contains(node))
            return [];

        return edgeLabel == null
            ? node.IncomingEdges
            : node.IncomingEdges.Where(e => e.Label == edgeLabel);
    }

    /// <summary>
    /// Removes a node and all its connected edges from the graph using the Try-Pattern
    /// </summary>
    public bool TryRemoveNode(Node node)
    {
        if (!_nodes.Contains(node))
            return false;

        // Remove all connected edges first
        var edgesToRemove = node.OutgoingEdges.Concat(node.IncomingEdges).ToList();
        foreach (var edge in edgesToRemove)
        {
            TryRemoveEdge(edge);
        }

        // Remove from label index
        if (_nodesByLabel.TryGetValue(node.Label, out var labelList))
        {
            labelList.Remove(node);
            if (labelList.Count == 0)
            {
                _nodesByLabel.Remove(node.Label);
            }
        }

        // Remove from main collection
        _nodes.Remove(node);
        return true;
    }

    /// <summary>
    /// Removes an edge from the graph using the Try-Pattern
    /// </summary>
    public bool TryRemoveEdge(Edge edge)
    {
        if (!_edges.Contains(edge))
            return false;

        // Remove from adjacency lists
        edge.From.OutgoingEdges.Remove(edge);
        edge.To.IncomingEdges.Remove(edge);

        // Remove from label index
        if (_edgesByLabel.TryGetValue(edge.Label, out var labelList))
        {
            labelList.Remove(edge);
            if (labelList.Count == 0)
            {
                _edgesByLabel.Remove(edge.Label);
            }
        }

        // Remove from main collection
        _edges.Remove(edge);
        return true;
    }

    /// <summary>
    /// Gets statistics about the graph
    /// </summary>
    public (int NodeCount, int EdgeCount, int NodeLabels, int EdgeLabels) GetStatistics() =>
        (_nodes.Count, _edges.Count, _nodesByLabel.Count, _edgesByLabel.Count);
}