using System.Dynamic;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Enumeration Type
/// </summary>
public sealed class EnumType(
    string Name,
    UnderlyingIntegerType UnderlyingType,
    bool IsFlags = false) : EdmElement(Name), IEdmPropertyType, IEdmElementCreator
{
    public UnderlyingIntegerType UnderlyingType { get; } = UnderlyingType;
    public bool IsFlags { get; } = IsFlags;

    private readonly List<Member> _members = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Member> Members => _members;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public Schema? ParentSchema { get; internal set; }

    public override string TargetPathSegment => $".{Name}";
    protected override EdmElement? Parent => ParentSchema;

    internal void AddMember(Member member)
    {
        member.ParentEnumType = this;
        _members.Add(member);
    }

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [.. _members, .. _annotations];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        yield return ("UnderlyingType", UnderlyingType);
        yield return ("IsFlags", IsFlags);
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        // https://docs.oasis-open.org/odata/odata-csdl-xml/v4.02/csd01/odata-csdl-xml-v4.02-csd01.html#ElementedmEnumType.17
        var enumType = new EnumType(
                startElement.Get("Name"),
                startElement.Parse<UnderlyingIntegerType>("UnderlyingType"),
                startElement.Parse<bool>("IsFlags"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddEnumType(enumType);
        }

        stack.Push(enumType);
    }
}

/// <summary>
/// Represents a Member of an Enumeration Type
/// </summary>
public sealed class Member(
    string Name,
    uint? Value = null) : EdmElement(Name), IEdmElementCreator
{
    public uint? Value { get; } = Value;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public EnumType? ParentEnumType { get; internal set; }

    public override string TargetPathSegment => $"/{Name}";
    
    protected override EdmElement? Parent => ParentEnumType;

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    public override IReadOnlyCollection<EdmElement> Children => [
        .. _annotations,
    ];

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", TargetPath);
        if (Value != null)
        {
            yield return ("Value", Value);
        }
    }

    public static void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions)
    {
        var member = new Member(
            startElement.Get("Name"),
            startElement.ParseNullable<uint>("Value"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EnumType enumType)
        {
            enumType.AddMember(member);
        }

        stack.Push(member);
    }
}
