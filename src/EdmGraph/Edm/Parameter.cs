using Serilog;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Parameter for an Action or Function
/// </summary>
public sealed class Parameter(string Name) : EdmElement(Name), IEdmElementFactory<Parameter>
{
    public IEdmPropertyType Type { get; internal set; } = default!; // Set by ModelBuilder

    public bool Nullable { get; internal set; }
    public int? MaxLength { get; internal set; }
    public bool Unicode { get; internal set; }
    public int? Precision { get; internal set; }
    public Scale? Scale { get; internal set; }
    public int? SRID { get; internal set; }

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public IEdmElement ParentElement { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentElement;

    public override IReadOnlyCollection<IEdmElement> Children => _annotations.Cast<IEdmElement>().ToList();

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("Type", Type);
        yield return ("Nullable", Nullable);
        if (MaxLength.HasValue)
        {
            yield return ("MaxLength", MaxLength.Value);
        }
        yield return ("Unicode", Unicode);
        if (Precision.HasValue)
        {
            yield return ("Precision", Precision.Value);
        }
        if (Scale != null)
        {
            yield return ("Scale", Scale);
        }
        if (SRID.HasValue)
        {
            yield return ("SRID", SRID.Value);
        }
    }

    #endregion

    public static Parameter Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var parameter = new Parameter(
            startElement.Get("Name"));

        parameter.ParentElement = parent;

        // Initialize nullable and facets from attributes
        parameter.Nullable = startElement.ParseNullable<bool>("Nullable") ?? false;
        parameter.MaxLength = startElement.ParseNullable<int>("MaxLength");
        parameter.Unicode = startElement.ParseNullable<bool>("Unicode") ?? false;
        parameter.Precision = startElement.ParseNullable<int>("Precision");
        parameter.Scale = startElement.ParseNullable<Scale>("Scale");
        parameter.SRID = startElement.ParseNullable<int>("SRID");

        switch (parent)
        {
            case Function function:
                function.AddParameter(parameter);
                break;
            case Action action:
                action.AddParameter(parameter);
                break;
        }

        actions.Add(Priorities.Type, parameter, startElement.Get("Type"), SetTypeAction);
        return parameter;

        // static helper method for the deferred action to set the Type property of Parameter
        static void SetTypeAction(Parameter parameter, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                // element.IsCollection = true;
            }
            parameter.Type = resolver.Resolve<IEdmPropertyType>(path);
            if (parameter.Type is null)
            {
                Log.Warning("'{path}' not found while trying to set the Type of {element}", path, parameter);
            }

        }
    }
}
