namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Annotation
/// </summary>
public sealed class Annotation(
    string? Qualifier = null) : EdmElement("Annotation")
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

}
