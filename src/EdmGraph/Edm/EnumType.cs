using System.Dynamic;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Enumeration Type
/// </summary>
public sealed class EnumType(
    string Name,
    UnderlyingIntegerType UnderlyingType,
    bool IsFlags = false) : IEdmElement, IEdmPropertyType, IEdmElementCreator<EnumType>
{
    public UnderlyingIntegerType UnderlyingType { get; } = UnderlyingType;
    public bool IsFlags { get; } = IsFlags;

    private readonly List<Member> _members = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Member> Members => _members;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public Schema? ParentSchema { get; internal set; }

    internal void AddMember(Member member)
    {
        member.ParentEnumType = this;
        _members.Add(member);
    }

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region IEdmElement

    string? IEdmElement.Name => Name;
    IEdmElement? IEdmElement.Parent => ParentSchema;
    string IEdmElement.TargetPathSegment => $".{Name}";
    LineInfo IEdmElement.LineInfo { get; set; } = new();
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [.. _members, .. _annotations];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Name", ((IEdmElement)this).TargetPath);
        yield return ("UnderlyingType", UnderlyingType);
        yield return ("IsFlags", IsFlags);
    }

    #endregion

    public static EnumType Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        // https://docs.oasis-open.org/odata/odata-csdl-xml/v4.02/csd01/odata-csdl-xml-v4.02-csd01.html#ElementedmEnumType.17
        var enumType = new EnumType(
                startElement.Get("Name"),
                startElement.Parse<UnderlyingIntegerType>("UnderlyingType"),
                startElement.Parse<bool>("IsFlags"));

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddEnumType(enumType);
        }

        stack.Push(enumType);
        return enumType;
    }
}

/// <summary>
/// Represents a Member of an Enumeration Type
/// </summary>
public sealed class Member(string Name, uint? Value = null) : IEdmElement, IEdmElementCreator<Member>
{
    public uint? Value { get; } = Value;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public EnumType? ParentEnumType { get; internal set; }

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);

    #region IEdmElement

    string? IEdmElement.Name => Name;
    IEdmElement? IEdmElement.Parent => ParentEnumType;
    string IEdmElement.TargetPathSegment => $"/{Name}";
    LineInfo IEdmElement.LineInfo { get; set; } = new();
    IReadOnlyCollection<IEdmElement> IEdmElement.Children => [
        .. _annotations,
    ];

    IEnumerable<(string, object?)> IEdmElement.GetBasicProperties()
    {
        yield return ("Name", ((IEdmElement)this).TargetPath);
        if (Value != null)
        {
            yield return ("Value", Value);
        }
    }

    #endregion

    public static Member Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions)
    {
        var member = new Member(
            startElement.Get("Name"),
            startElement.ParseNullable<uint>("Value"));

        if (stack.TryPeek(out var parent) && parent is EnumType enumType)
        {
            enumType.AddMember(member);
        }

        stack.Push(member);
        return member;
    }
}
