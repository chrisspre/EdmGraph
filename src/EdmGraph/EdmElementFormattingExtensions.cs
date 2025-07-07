
namespace EdmGraph;

using System.Linq.Expressions;
using EdmGraph.Edm;

public static class EdmElementExtensions
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

   
}

public static class EdmElementFormattingExtensions
{
    public static void WriteAsciiTree(this EdmElement root, TextWriter writer)
    {
        var stack = new Stack<(string, bool, EdmElement)>();
        foreach (var (child, last) in WithLast(root.Children))
        {
            stack.Push(("", last, child));
        }

        while (stack.TryPop(out var entry))
        {
            var (indent, isLast, element) = entry;

            writer.WriteLine($"{indent}{(isLast ? _corner : _cross)}{element} {element.LineInfo}");
            indent += isLast ? _space : _vertical;
            // foreach (var (child, last) in WithLast(element.Children))
            // {
            //     stack.Push((indent, last, child));
            // }
            var children = element.Children.ToList();
            for (int i = children.Count - 1; i >= 0; i--)
            {
                stack.Push((indent, i == children.Count - 1, children[i]));
            }
        }

        static IEnumerable<(T, bool)> WithLast<T>(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            T current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (current, false);
                current = enumerator.Current;
            }
            yield return (current, true);
        }
    }

    private const string _cross = " ├─";
    private const string _corner = " └─";
    private const string _vertical = " │ ";
    private const string _space = "   ";
    // private const string _cross     = " +-";
    // private const string _corner    = " +-";
    // private const string _vertical  = " | ";
    // private const string _space     = "   ";
   
}
