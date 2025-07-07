using EdmGraph.Edm;
using Serilog;

namespace EdmGraph;


public interface IElementResolver
{
    /// <summary>
    /// Resolves an element by its full name relative to the root element.
    /// </summary>
    /// <param name="name">The full name (target path) of the element to resolve.</param>
    /// <returns>The resolved element, or null if not found.</returns>
    T Resolve<T>(IEdmElement root, string name) where T : class, IEdmElement;

    /// <summary>
    /// Resolves an element by its full name relative to the model root (a DataService instance).
    /// </summary>
    T Resolve<T>(string name) where T : class, IEdmElement;
}


public class Priorities
{
    public const int Type = 2;
    public const int BaseType = 3;
    public const int PropertyRef = 4;
    public const int Partner = 5;
}

public class DeferredActionRegistry
{
    /// <summary>
    /// A priority queue to store actions that will be applied later.
    /// this is strictly not necessary, since all actions get enqueued before anything is dequeued, 
    /// but PriorityQueue is available in .NET 6+ and provides a convenient way to manage the order of actions.
    /// </summary>
    private readonly PriorityQueue<Action<IElementResolver>, int> _actions = new();


    public void Add<T>(int priority, T element, string name, Action<T, string, IElementResolver> action) where T : IEdmElement
    {
        // Register an action to be applied later
        _actions.Enqueue(resolver => action(element, name, resolver), priority);
    }

    public void ApplyActions(DataServices model)
    {
        var resolver = new Resolver(model);

        while(_actions.TryDequeue(out var action, out var _))
        {        
            action(resolver);
        }
    }

    internal void Add(object propertyRef1, PropertyRef propertyRef2, string v, Action<PropertyRef, string, IElementResolver> value)
    {
        throw new NotImplementedException();
    }

    internal class Resolver(DataServices dataServices) : IElementResolver
    {
        public T Resolve<T>(string name) where T : class, IEdmElement
        {
            if (name.StartsWith("Edm.") && PrimitiveTypes.FromName(name) is PrimitiveType primitiveType)
            {
                // Handle primitive types directly
                return (T)(IEdmElement)primitiveType;
            }

            if (name.StartsWith("Org.OData."))
            {
                Log.Verbose("Skipping vocabulary term resolution for '{Name}'", name);
                return default(T)!; // Skip vocabulary terms for now

            }

            if (!dataServices.TryFind(name, out var element))
            {
                // Log.Warning("Element '{Name}' not found", name);
                return default(T)!;
            }
            else if (element is T typedElement)
            {
                // Log.Information("Resolved element '{Name}' to type '{Type}' {element}", name, typeof(T).Name, element);
                return typedElement;
            }
            else
            {
                Log.Warning("Element '{Name}' not of type '{Type}'", name, typeof(T).Name);
                return default(T)!;
            }
        }

        public T Resolve<T>(IEdmElement root, string name) where T : class, IEdmElement
        {
            if (!root.TryFind(name, out var element))
            {
                Log.Warning("Element '{Name}' not found", name);
                return default(T)!;
            }
            else if (element is T typedElement)
            {
                Log.Verbose("Resolved element '{Name}' to type '{Type}' {element}", name, typeof(T).Name, element);
                return typedElement;
            }
            else
            {
                Log.Warning("Element '{Name}' not of type '{Type}'", name, typeof(T).Name);
                return default(T)!;
            }
        }
    }
}

public static class IResolverExtensions
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