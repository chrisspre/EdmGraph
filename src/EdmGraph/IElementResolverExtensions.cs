using EdmGraph.Edm;

namespace EdmGraph;

public static class IElementResolverExtensions
{
    public static PropertyPath ResolvePropertyPath(this IElementResolver resolver, IEdmElement element, string path)
    {
        var segments = path.Split('/');
        // assert segments.Length >= 1;

        var current = element;
        var prop = resolver.Resolve<IProperty>(current, segments[0]);
        var result = new PropertyPath(prop);

        // iterate over the remaining segments and r
        // esolve each property based on the previous one's type
        for (int i = 1; i < segments.Length; i++)
        {
            current = prop.Type;
            prop = resolver.Resolve<Property>(current, segments[i]);
            result.AddSegment(prop);
        }

        return result;
    }
}