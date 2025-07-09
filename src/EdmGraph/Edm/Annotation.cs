
using Serilog;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotation
/// </summary>
public sealed class Annotation(string? Qualifier = null) : EdmElement("Annotation"), IEdmElementFactory<Annotation>
{
    public Term Term { get; internal set; } = default!;
    
    public string? Qualifier { get; } = Qualifier;

    public IEdmElement ParentElement { get; internal set; } = default!; // Set in ModelBuilder/Create
    
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public AnnotationExpression? Expression { get; init; }

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentElement;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => 
        $"@{Term}" + (Qualifier != null ? $"#{Qualifier}" : "");

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Term", Term);
        if (Qualifier != null)
        {
            yield return ("Qualifier", Qualifier);
        }
    }

    #endregion

    public static Annotation Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var annotation = new Annotation(
                startElement.Get("Qualifier", null));

        // Handle the most common parent types that support annotations
        switch (parent)
        {
            case EntityType et:
                et.AddAnnotation(annotation);
                break;
            case ComplexType ct:
                ct.AddAnnotation(annotation);
                break;
            case Property prop:
                prop.AddAnnotation(annotation);
                break;
            case NavigationProperty nprop:
                nprop.AddAnnotation(annotation);
                break;
            case EntitySet es:
                es.AddAnnotation(annotation);
                break;
            case Singleton singleton:
                singleton.AddAnnotation(annotation);
                break;
            case Function func:
                func.AddAnnotation(annotation);
                break;
            case Action action:
                action.AddAnnotation(annotation);
                break;
            case Parameter param:
                param.AddAnnotation(annotation);
                break;
            case EnumType enumType:
                enumType.AddAnnotation(annotation);
                break;
            case Member member:
                member.AddAnnotation(annotation);
                break;
            case EntityContainer container:
                container.AddAnnotation(annotation);
                break;
            case Annotations annotations:
                annotations.AddAnnotation(annotation);
                break;
            case Reference reference:
                reference.AddAnnotation(annotation);
                break;
            case DataServices dataServices:
                dataServices.AddAnnotation(annotation);
                break;
            // Add more cases as needed
            default:
                // If the parent type is not recognized
                Log.Warning($"Unhandled parent type {parent} for annotation {annotation}", parent, annotation);
                break;
        }

        // deferred action for Term property resolution
        actions.Add(Priorities.Type, annotation, startElement.Get("Term"), static (element, path, resolver) =>
        {
            element.Term = resolver.Resolve<Term>(path);
        });

        return annotation;
    }
}
