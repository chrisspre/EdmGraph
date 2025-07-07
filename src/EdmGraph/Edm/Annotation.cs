
using Serilog;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotation
/// </summary>
public sealed class Annotation(string? Qualifier = null) : IEdmElement, IEdmElementCreator<Annotation>
{
    public Term Term { get; internal set; } = default!;
    
    public string? Qualifier { get; } = Qualifier;

    public IEdmElement? ParentElement { get; internal set; }
    
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    // // Expression properties (one of these will be set)
    // public string? Binary { get; init; }
    // public bool? Bool { get; init; }
    // public string? Date { get; init; }
    // public string? DateTimeOffset { get; init; }
    // public decimal? Decimal { get; init; }
    // public string? Duration { get; init; }
    // public string? EnumMember { get; init; }
    // public double? Float { get; init; }
    // public string? Guid { get; init; }
    // public long? Int { get; init; }
    // public string? String { get; init; }
    // public string? TimeOfDay { get; init; }
    // public string? AnnotationPath { get; init; }
    // public string? ModelElementPath { get; init; }
    // public string? NavigationPropertyPath { get; init; }
    // public string? PropertyPath { get; init; }
    // public string? Path { get; init; }
    // public string? UrlRef { get; init; }

    public AnnotationExpression? Expression { get; init; }

    public string Name { get; internal set; } = "Annotation";

    public string TargetPathSegment => 
        $"@{Term}" + (Qualifier != null ? $"#{Qualifier}" : "");
    
    public string? TargetPath
    {
        get
        {
            if (TargetPathSegment is null)
            {
                return null;
            }
            return Parent?.TargetPath + TargetPathSegment;
        }
    }
    
    public IEdmElement? Parent => ParentElement;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public IReadOnlyCollection<IEdmElement> Children => [
        .. _annotations
    ];

    public IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Term", Term);
        if (Qualifier != null)
        {
            yield return ("Qualifier", Qualifier);
        }
        // // Expression values could be added here if needed
        // if (String != null)
        // {
        //     yield return ("String", String);
        // }
        // if (Bool.HasValue)
        // {
        //     yield return ("Bool", Bool.Value);
        // }
        // if (Int.HasValue)
        // {
        //     yield return ("Int", Int.Value);
        // }
    }

    public LineInfo LineInfo { get; set; }

    public override string ToString()
    {
        var properties = GetBasicProperties();
        var propertyString = string.Join(", ", properties.Select(p => $"{p.Item1}={p.Item2}"));
        return $@"<{GetType().Name} {propertyString} \>";
    }

    public static Annotation Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var annotation = new Annotation(
                startElement.Get("Qualifier", null));

        // Handle the most common parent types that support annotations
        if (stack.TryPeek(out var parent))
        {
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
        }

        // deferred action for Term property resolution
        actions.Add(Priorities.Type, annotation, startElement.Get("Term"), static (element, path, resolver) =>
        {
            element.Term = resolver.Resolve<Term>(path);
        });

        return annotation;
    }
}
