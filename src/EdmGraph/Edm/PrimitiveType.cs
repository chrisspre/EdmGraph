namespace EdmGraph.Edm;

public sealed class PrimitiveType(string Name) : EdmElement(Name), IPropertyType, IEdmElement
{
    public override string TargetPathSegment => $".{Name}";

    public override IReadOnlyCollection<EdmElement> Children => [];

    protected override EdmElement? Parent => ParentEnumType;

    public EnumType? ParentEnumType { get; internal set; }

    protected override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return ("Name", Name);
    }
}