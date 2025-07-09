namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Function Import
/// </summary>
public sealed class FunctionImport(string Name, string Function, string? EntitySet = null, bool IncludeInServiceDocument = false) 
    : EdmElement(Name), IEdmElementFactory<FunctionImport>
{
    public string Function { get; } = Function;
    public string? EntitySet { get; } = EntitySet;
    public bool IncludeInServiceDocument { get; } = IncludeInServiceDocument;

    public EntityContainer ParentEntityContainer { get; internal set; } = default!; // Set in ModelBuilder/Create

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

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
        yield return ("Function", Function);
        if (EntitySet != null)
        {
            yield return ("EntitySet", EntitySet);
        }
        yield return ("IncludeInServiceDocument", IncludeInServiceDocument);
    }

    #endregion

    public static FunctionImport Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var functionImport = new FunctionImport(
           startElement.Get("Name"),
           startElement.Get("Function"),
           startElement.Get("EntitySet", null),
           startElement.Parse<bool>("IncludeInServiceDocument", false));

        if (parent is EntityContainer entityContainer)
        {
            entityContainer.AddFunctionImport(functionImport);
        }

        // TODO: Add deferred action for Function property resolution
        // actions.Add(functionImport, startElement.Get("Function"), static (element, path, resolver) => { 
        //     element.Function = resolver.Resolve<Function>(path);
        // });

        // TODO: Add deferred action for EntitySet property resolution if EntitySet is not null
        var entitySet = startElement.Get("EntitySet", null);
        if (entitySet is not null)
        {
            // actions.Add(functionImport, entitySet, static (element, path, resolver) => { 
            //     element.EntitySet = resolver.Resolve<EntitySet>(path);
            // });
        }

        return functionImport;
    }
}
