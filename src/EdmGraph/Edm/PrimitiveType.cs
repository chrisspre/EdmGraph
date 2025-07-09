namespace EdmGraph.Edm;


// this intentionally does not implement IEdmElementFactory
// as it is not created from CSDL, but rather directly in ModelBuilder
public sealed class PrimitiveType(string Name) : EdmElement(Name), IEdmPropertyType
{
    public EnumType ParentEnumType { get; internal set; } = default!; // Set in ModelBuilder/Create

    #region EdmElement abstract member implementation

    public override IEdmElement Parent => ParentEnumType;

    public override string TargetPathSegment => $".{Name}";

    public override IReadOnlyCollection<IEdmElement> Children => [];

    public override IEnumerable<(string, object?)> GetBasicProperties()
    {
        yield return (nameof(Name), Name);
    }

    #endregion
}
