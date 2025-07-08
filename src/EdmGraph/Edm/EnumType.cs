using System.Dynamic;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Enumeration Type
/// </summary>
public sealed class EnumType(
    string Name,
    UnderlyingIntegerType UnderlyingType,
    bool IsFlags = false) : EdmElement(Name), IEdmPropertyType, IEdmElementFactory<EnumType>
{
    public UnderlyingIntegerType UnderlyingType { get; } = UnderlyingType;
    public bool IsFlags { get; } = IsFlags;

    private readonly List<Member> _members = [];
    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Member> Members => _members;
    public IReadOnlyCollection<Annotation> Annotations => _annotations;

    public Schema ParentSchema { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddMember(Member member)
    {
        member.ParentEnumType = this;
        _members.Add(member);
    }

    internal void AddAnnotation(Annotation annotation)
    {
        _annotations.Add(annotation);
        annotation.ParentElement = this;
    }

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentSchema;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _members, .. _annotations];

    public override string TargetPathSegment => $".{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        yield return ("UnderlyingType", UnderlyingType);
        yield return ("IsFlags", IsFlags);
    }

    #endregion

    public static EnumType Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        // https://docs.oasis-open.org/odata/odata-csdl-xml/v4.02/csd01/odata-csdl-xml-v4.02-csd01.html#ElementedmEnumType.17
        var enumType = new EnumType(
                startElement.Get("Name"),
                startElement.Parse<UnderlyingIntegerType>("UnderlyingType"),
                startElement.Parse<bool>("IsFlags"));

        if (parent is Schema schema)
        {
            schema.AddEnumType(enumType);
        }

        return enumType;
    }
}

/// <summary>
/// Represents a Member of an Enumeration Type
/// </summary>
public sealed class Member(string Name, uint? Value = null) : EdmElement(Name), IEdmElementFactory<Member>
{
    public uint? Value { get; } = Value;

    private readonly List<Annotation> _annotations = [];

    public IReadOnlyCollection<Annotation> Annotations => _annotations;
    public EnumType ParentEnumType { get; internal set; } = default!; // Set in ModelBuilder/Create

    internal void AddAnnotation(Annotation annotation) => _annotations.Add(annotation);


    #region EdmElement abstract member implementation
    public override IEdmElement Parent => ParentEnumType;

    public override IReadOnlyCollection<IEdmElement> Children => [.. _annotations];

    public override string TargetPathSegment => $"/{Name}";

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
        if (Value.HasValue) yield return ("Value", Value.Value);
    }

    #endregion
    
    public static Member Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions)
    {
        var member = new Member(
            startElement.Get("Name"),
            startElement.ParseNullable<uint>("Value"));

        if (parent is EnumType enumType)
        {
            enumType.AddMember(member);
        }

        return member;
    }
}
