
using Serilog;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotation
/// </summary>
public sealed class Annotation(string? Qualifier = null) : EdmElement("Annotation"), IEdmElementCreator
{
    public Term Term { get; internal set; } = default!;
    
    public string? Qualifier { get; } = Qualifier;

    public EdmElement? ParentElement { get; internal set; }
    
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

    public override string TargetPathSegment => 
        $"@{Term}" + (Qualifier != null ? $"#{Qualifier}" : "");
    
    protected override EdmElement? Parent => ParentElement;

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations
    ];


    protected override IEnumerable<(string, object?)> GetBasicProperties()
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

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var annotation = new Annotation(
                startElement.Get("Qualifier", null))
        {
            LineInfo = startElement.LineInfo
        };

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

        stack.Push(annotation);
    }
}
