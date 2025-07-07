
namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Navigation Property Binding
/// </summary>
public sealed class NavigationPropertyBinding(string Path, string Target) : IEdmElement, IEdmElementCreator<NavigationPropertyBinding>
{
    public string Path { get; } = Path;
    public string Target { get; } = Target;

    public IEdmElement? ParentElement { get; internal set; }

    #region IEdmElement

    public LineInfo LineInfo { get; set; }

    string? IEdmElement.Name => "NavigationPropertyBinding";

    IEdmElement? IEdmElement.Parent => ParentElement;
    
    string IEdmElement.TargetPathSegment => "/NavigationPropertyBinding";

    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [
        // NavigationPropertyBinding does not have children
    ];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Path", Path);
        yield return ("Target", Target);
    }

    #endregion

    public static NavigationPropertyBinding Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var navigationPropertyBinding = new NavigationPropertyBinding(
           startElement.Get("Path"),
           startElement.Get("Target"));

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntitySet entitySet:
                    entitySet.AddNavigationPropertyBinding(navigationPropertyBinding);
                    break;
                case Singleton singleton:
                    singleton.AddNavigationPropertyBinding(navigationPropertyBinding);
                    break;
            }
        }

        // TODO: Add deferred action for Path property resolution (NavigationProperty)
        // actions.Add(navigationPropertyBinding, startElement.Get("Path"), static (element, path, resolver) => { 
        //     element.NavigationProperty = resolver.Resolve<NavigationProperty>(path);
        // });

        // TODO: Add deferred action for Target property resolution (EntitySet)
        // actions.Add(navigationPropertyBinding, startElement.Get("Target"), static (element, path, resolver) => { 
        //     element.Target = resolver.Resolve<EntitySet>(path);
        // });

        return navigationPropertyBinding;
    }
}
