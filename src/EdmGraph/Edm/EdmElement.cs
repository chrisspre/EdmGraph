namespace EdmGraph.Edm;


/// <summary>
/// Base class for all EDM model elements that support creation from CSDL tokens
/// </summary>
public abstract class EdmElement(string Name) : IEdmElement
{
    public string Name { get; internal set;} = Name;

    /// <summary>
    /// Gets the target path segment for this element, including the appropriate separator
    /// </summary>
    public abstract string TargetPathSegment { get; }

    /// <summary>
    /// Gets the full target path for this element or null if the element has no target path.
    /// The target path is constructed its target path segment and the targe path of the parent
    /// </summary>
    public string? TargetPath
    {
        get
        {
            if(TargetPathSegment is null) 
            {
                return null;
            }
            return Parent?.TargetPath + TargetPathSegment;
        }
    }

    /// <summary>
    /// Gets the parent element of this EDM element
    /// </summary>
    protected abstract EdmElement? Parent { get; }


    public abstract IReadOnlyCollection<EdmElement> Children { get; }

    protected abstract IEnumerable<(string, object?)> GetBasicProperties() ;

    /// <summary>
    /// Returns a string representation of this EDM element
    /// </summary>
    public override string ToString() 
    {
        var properties = GetBasicProperties();
        var propertyString = string.Join(", ", properties.Select(p => $"{p.Item1}={p.Item2}"));
        return $@"<{GetType().Name} {propertyString} \>";
    }


    public  LineInfo LineInfo { get; init;}

#region explicit IEdmElement implementation    
    EdmElement? IEdmElement.Parent => Parent!;

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties() => GetBasicProperties();
    
    string IEdmElement.Name => Name;

    string? IEdmElement.TargetPath => TargetPath;

    IReadOnlyCollection<EdmElement> IEdmElement.Children => Children;
#endregion
}


public static class EdmElementExtensions
{
    public static bool TryFind(this IEdmElement root, string path, [MaybeNullWhen(false)] out EdmElement result)
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
    private static bool TryFind(IEnumerable<EdmElement> nodes, IReadOnlyList<string> segments, int i, [MaybeNullWhen(false)] out EdmElement node)
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
    private static HashSet<string> NodeSegmentNames(EdmElement element)
    {
        static HashSet<string> NoneNull(params string?[] values) => values.Where(v => v is not null).ToHashSet()!;

        return element switch
        {
            Schema schema =>
                NoneNull(schema.Namespace, schema.Alias),
            Annotation annotation =>
                [annotation.Qualifier is not null ? $"{annotation.Term}#{annotation.Qualifier}" : $"{annotation.Term}"],
            Function function =>
                [function.Name, $"{function.Name}({string.Join(",", function.Parameters.Select(p => p.Type))}))"],
            Action action =>
                [action.Name, $"{action.Name}({string.Join(",", action.Parameters.Select(p => p.Type))}))"],            
            _ =>
                [ element.Name],
        };
    }
}