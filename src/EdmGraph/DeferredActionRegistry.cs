using EdmGraph.Edm;
using Serilog;

namespace EdmGraph;


internal interface IElementResolver
{
    /// <summary>
    /// Resolves an element by its full name.
    /// </summary>
    /// <param name="name">The full name (target path) of the element to resolve.</param>
    /// <returns>The resolved element, or null if not found.</returns>
    T Resolve<T>(string name) where T : class, IEdmElement;
    T Resolve<T>(EdmElement root, string name) where T : class, IEdmElement;
}

internal class DeferredActionRegistry
{
    private readonly List<Action<IElementResolver>> _actions = [];

    public void Add<T>(T element, string name, Action<T, string, IElementResolver> action) where T : EdmElement
    {
        // Register an action to be applied later
        _actions.Add(resolver => action(element, name, resolver));
    }

    public void ApplyActions(DataServices model)
    {
        var resolver = new Resolver(model);

        foreach (var action in _actions)
        {
            action(resolver);
        }
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

        public T Resolve<T>(EdmElement root, string name) where T : class, IEdmElement
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
