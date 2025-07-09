namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Action Import
/// </summary>
public sealed class ActionImport(string Name, string Action, string? EntitySet = null) : EdmElement(Name), IEdmElementFactory<ActionImport>
{
    public EntityContainer ParentEntityContainer { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public string Action { get; } = Action;
    public string? EntitySet { get; } = EntitySet;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEntityContainer;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("Action", Action);
        if (EntitySet != null)
        {
            yield return ("EntitySet", EntitySet);
        }
    }

    #endregion

    public static ActionImport Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var actionImport = new ActionImport(
            startElement.Get("Name"),
            startElement.Get("Action"),
            startElement.Get("EntitySet", null));

        if (parent is EntityContainer entityContainer)
        {
            entityContainer.AddActionImport(actionImport);
        }

        // TODO: Add deferred action for Action property resolution
        // actions.Add(actionImport, startElement.Get("Action"), static (element, path, resolver) => { 
        //     element.Action = resolver.Resolve<Action>(path);
        // });

        // TODO: Add deferred action for EntitySet property resolution if EntitySet is not null
        var entitySet = startElement.Get("EntitySet", null);
        if (entitySet is not null)
        {
            // actions.Add(actionImport, entitySet, static (element, path, resolver) => { 
            //     element.EntitySet = resolver.Resolve<EntitySet>(path);
            // });
        }

        return actionImport;
    }
}
