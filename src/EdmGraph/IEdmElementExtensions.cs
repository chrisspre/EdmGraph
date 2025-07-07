namespace EdmGraph;
using EdmGraph.Edm;

public static class IEdmElementExtensions
{
    public static IEnumerable<IEdmElement> DescendantsAndSelf(this IEdmElement element)
    {
        var stack = new Stack<IEdmElement>();
        stack.Push(element);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            foreach (var child in current.Children.Reverse())
            {
                stack.Push(child);
            }
        }
    }

    public static bool TryFind(this IEdmElement root, string path, [MaybeNullWhen(false)] out IEdmElement result)
    {
        var segments = ParsePathSegments(path); 

        return TryFind(root.Children, segments, 0, out result);
    }

    /// <summary>
    /// Splits an OData path into segments, handling namespace/alias prefixes and special characters.
    /// </summary>
    /// <param name="path"></param>
    /// <remarks>
    /// This method splits paths :<br/>
    ///  - "graph.managedAppRegistration/userId" into ["graph", "managedAppRegistration", "userId"]<br/>
    ///  - "microsoft.graph.user" into ["microsoft.graph", "user"]<br/>
    ///  - "graph/user" into ["graph", "user"]<br/>
    ///  - "microsoft.graph.managedAppRegistration/userId" into ["microsoft.graph", "managedAppRegistrationm", "userId"]<br/>
    /// </remarks>
    private static List<string> ParsePathSegments(string path)
    {
        var segments = new List<string>();

        if (string.IsNullOrEmpty(path))
            return segments;

        var firstSlashOrAt = path.IndexOfAny(['/', '@']);

        if (firstSlashOrAt == -1)
        {
            // No '/' or '@' found, check for dots
            var lastDot = path.LastIndexOf('.');
            if (lastDot > 0)
            {
                segments.Add(path[..lastDot]);
                segments.Add(path[(lastDot + 1)..]);
            }
            else
            {
                segments.Add(path);
            }
        }
        else
        {
            // Find the last dot before the first '/' or '@'
            var prefix = path[..firstSlashOrAt];
            var lastDotInPrefix = prefix.LastIndexOf('.');

            if (lastDotInPrefix > 0)
            {
                segments.Add(prefix[..lastDotInPrefix]);
                path = prefix[(lastDotInPrefix + 1)..] + path[firstSlashOrAt..];
            }

            // Split the remaining path by '.', '/', '@'
            var currentSegment = new System.Text.StringBuilder();
            foreach (var ch in path)
            {
                if (ch == '.' || ch == '/' || ch == '@')
                {
                    if (currentSegment.Length > 0)
                    {
                        segments.Add(currentSegment.ToString());
                        currentSegment.Clear();
                    }
                }
                else
                {
                    currentSegment.Append(ch);
                }
            }

            if (currentSegment.Length > 0)
            {
                segments.Add(currentSegment.ToString());
            }
        }

        return segments;
    }

    // Recursive helper method to find a node by path segments
    private static bool TryFind(IEnumerable<IEdmElement> nodes, IReadOnlyList<string> segments, int i, [MaybeNullWhen(false)] out IEdmElement node)
    {
        if (i == segments.Count)
        {
            node = null!;
            return false;
        }

        foreach (var child in nodes)
        {
            if (NodeSegmentNames(child).Contains(segments[i]))
            {
                if (i == segments.Count - 1) // last segment
                {
                    node = child;
                    return true;
                }
                else
                {
                    if (TryFind(child.Children, segments, i + 1, out node))
                    {
                        return true;
                    }
                }
            }
        }

        node = null!;
        return false;
    }

    /// <summary>
    /// collects all alternative segment names for a node, which are used to find the node by path segments
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private static HashSet<string> NodeSegmentNames(IEdmElement element)
    {
        static HashSet<string> NoneNull(params string?[] values) => values.Where(v => v is not null).ToHashSet()!;

        return element switch
        {
            Schema schema =>
                NoneNull(schema.Namespace, schema.Alias),
            Annotation annotation =>
                [annotation.Qualifier is not null ? $"{annotation.Term}#{annotation.Qualifier}" : $"{annotation.Term}"],
            Function function =>
                [.. NoneNull(element.Name), $"{element.Name}({string.Join(",", function.Parameters.Select(p => p.Type))}))"],
            Action action =>
                [.. NoneNull(element.Name), $"{element.Name}({string.Join(",", action.Parameters.Select(p => p.Type))}))"],            
            _ =>
                [.. NoneNull(element.Name)],
        };
    }
}
