
namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Navigation Property Binding
/// </summary>
public sealed class NavigationPropertyBinding(string Path, string Target) : EdmElement("NavigationPropertyBinding"), IEdmElementCreator
{
    public string Path { get; } = Path;
    public string Target { get; } = Target;

    public EdmElement? ParentElement { get; internal set; }

    public override string TargetPathSegment => null!; // NavigationPropertyBinding does not have a target path segment on its own

    protected override EdmElement? Parent => ParentElement;

    public override IReadOnlyCollection<EdmElement> Children => [
        // NavigationPropertyBinding does not have children
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Path", Path);
        yield return ("Target", Target);
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var navigationPropertyBinding = new NavigationPropertyBinding(
           startElement.Get("Path"),
           startElement.Get("Target"))
        {
            LineInfo = startElement.LineInfo
        };

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

        stack.Push(navigationPropertyBinding);
    }
}
