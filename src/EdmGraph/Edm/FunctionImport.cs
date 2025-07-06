namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Function Import
/// </summary>
public sealed class FunctionImport(string Name, string Function, string? EntitySet = null, bool IncludeInServiceDocument = false) : EdmElement(Name), IEdmElementCreator
{
    public string Function { get; } = Function;
    public string? EntitySet { get; } = EntitySet;
    public bool IncludeInServiceDocument { get; } = IncludeInServiceDocument;

    public EntityContainer? ParentEntityContainer { get; internal set; }

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public override string TargetPathSegment => $"/{Name}";
    protected override EdmElement? Parent => ParentEntityContainer;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [.. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("Function", Function);
        if (EntitySet != null)
        {
            yield return ("EntitySet", EntitySet);
        }
        yield return ("IncludeInServiceDocument", IncludeInServiceDocument);
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var functionImport = new FunctionImport(
           startElement.Get("Name"),
           startElement.Get("Function"),
           startElement.Get("EntitySet", null),
           startElement.Parse<bool>("IncludeInServiceDocument", false))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityContainer entityContainer)
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

        stack.Push(functionImport);
    }
}
