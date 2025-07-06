using System.Dynamic;

namespace EdmGraph.Edm;

/// <summary>
/// Represents an EDM Enumeration Type
/// </summary>
public sealed class EnumType(
    string Name,
    UnderlyingIntegerType UnderlyingType,
    bool IsFlags = false) : EdmElement(Name), IEdmPropertyType
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
}

/// <summary>
/// Represents a Member of an Enumeration Type
/// </summary>
public sealed class Member(
    string Name,
    uint? Value = null) : EdmElement(Name)
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
}
